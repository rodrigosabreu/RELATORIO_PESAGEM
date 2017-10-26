using PagedList;
using RELATORIO_PESAGEM.ModelsBALANCA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace RELATORIO_PESAGEM.Controllers
{
    public class RelatorioPesagemController : Controller
    {

        private BALANCATRANSBORDOEntities db = new BALANCATRANSBORDOEntities();
        public string limitePeriodo;


        private String ConverteDataParaIngles(String data)
        {

            string[] array = data.Split('/');

            string resp = array[1]+"-"+array[0]+"-"+array[2];

            return resp;
        }



        public bool restringeRelatorioPrefeituraData(string data)
        {

            bool resp = false;


            if(Session["cargo"].ToString() != "administrador")
            {

            

                    DateTime date = DateTime.Now;
                    DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                    firstDayOfMonth = firstDayOfMonth.AddDays(-10);
                    limitePeriodo = firstDayOfMonth.ToString("dd/MM/yyyy");
                    string primeiroDiaMes = firstDayOfMonth.ToString("MM-dd-yyyy");
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);


                    int result = DateTime.Compare(Convert.ToDateTime(primeiroDiaMes), Convert.ToDateTime(data));

                    Debug.WriteLine("#################################        " + primeiroDiaMes + " - " + data + "Resultado: "+result.ToString());


                    if (result < 0)
                        Debug.WriteLine("################################# SIM");
                    else if (result == 0)
                        Debug.WriteLine("################################# SIM");
                    else
                    {
                        Debug.WriteLine("################################# RELATORIO TRAVADO PELA DATA");

                        //throw new System.Exception("Período não permitido para emitir relatório.");
                        resp = true;
                    }



            }
            return resp;

        }



        // GET: RelatorioPesagem
        public ActionResult Index(int ? pagina, String data1, String data2)
        {


            try {

                if (Session["sessao"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }


                int tamanhoPagina = 10;
                int numeroPagina = pagina ?? 1;
                int ID_CLIENTE_LOGADO = Convert.ToInt32(Session["id_usuario"]);




                var list = (from cu in db.CLIENTEUSUARIO
                        where cu.idusuario == ID_CLIENTE_LOGADO
                        select new
                        {
                            IDCLIENTE = cu.idcliente
                        }).ToList();

                int?[] clientes_atrelados = new int?[list.Count];
                int i = 0;
                foreach (var item in list)
                {
                    clientes_atrelados[i] = item.IDCLIENTE;
                    i = i + 1;
                }




                if (Session["cargo"].ToString() == "administrador") {

                    var list2 = (from cu in db.CLIENTE
                            select new
                            {
                                IDCLIENTE = cu.ID
                            }).ToList(); ;



                    clientes_atrelados = new int?[list2.Count];
                    i = 0;
                    foreach (var item in list2)
                    {
                        clientes_atrelados[i] = item.IDCLIENTE;
                        i = i + 1;
                    }


                }



              



                if (!String.IsNullOrEmpty(data1) && !String.IsNullOrEmpty(data2))
                {
                    ViewBag.data1 = data1;
                    ViewBag.data2 = data2;

                    data1 = ConverteDataParaIngles(data1);
                    data2 = ConverteDataParaIngles(data2);

                }
                else
                {

                    DateTime data = DateTime.Now;
                    String hoje = data.ToString("dd/MM/yyyy");

                    ViewBag.data1 = hoje;
                    ViewBag.data2 = hoje;

                    //data1 = hoje;
                    //data2 = hoje;

                    data1 = ConverteDataParaIngles(hoje);
                    data2 = ConverteDataParaIngles(hoje);
                }



                if(restringeRelatorioPrefeituraData(data1) == true)
                {                    
                    return Content("<script language='javascript' type='text/javascript'>alert('Data inicial tem que ser a partir de " + limitePeriodo + ".');window.history.back();</script>");                   
                }

                data1 = data1 + " 00:00:00.000";
                data2 = data2 + " 23:59:59.999";

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);

                DateTime dt1 = Convert.ToDateTime(data1);
                DateTime dt2 = Convert.ToDateTime(data2);

                var lista = (from p in db.PESAGEM
                             where p.DATAPESAGEM >= dt1 && 
                                    p.DATAPESAGEM <= dt2 && 
                                    p.ATIVO == 1 &&
                                    clientes_atrelados.Contains(p.CLIENTE)

                             //join u in db.USUARIO on p.IDUSUARIO equals u.id into tmpU
                             //from u in tmpU.DefaultIfEmpty()

                             join t in db.TRANSPORTADORA on p.TRANSPORTADORA equals t.ID into tmpT
                             from t in tmpT.DefaultIfEmpty()

                             join prod in db.PRODUTO on p.PRODUTO equals prod.ID into tmpP
                             from prod in tmpP.DefaultIfEmpty()

                             join c in db.CLIENTE on p.CLIENTE equals c.ID into tmpC
                             from c in tmpC.DefaultIfEmpty()

                             select new
                             {
                                 TICKET = p.ID,
                                 BALANCEIRO = p.USUARIO,
                                 PLACA = p.PLACA,
                                 CACAMBA = p.CACAMBA,
                                 MOTORISTA = p.MOTORISTA,
                                 TRANPORTADORA = t.NOMETRANS,
                                 PRODUTO = prod.NOMEPROD,
                                 CLIENTE = c.NOME,

                                 DATAENTRADA = p.DATAPESAGEM,
                                 DATASAIDA = p.DATAPESAGEM2,

                                 PESOTARA = p.PESOTARA,
                                 PESOENTRADA = p.PESOTOTAL,
                                 PESOSAIDA = p.PESOTOTAL2,
                                 PESOLIQUIDO = p.PESOLIQUIDO,

                             })
                             .OrderByDescending(p => p.TICKET);


                ViewBag.totalPesagens = lista.ToList().Count();


                var novaList = new List<relatorio>();

                Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR", true);

                foreach (var item in lista)
                {
                    var cls = new relatorio();

                    cls.TICKET = item.TICKET;
                    cls.BALANCEIRO = item.BALANCEIRO == null ? "-" : item.BALANCEIRO.ToUpper();
                    cls.PLACA = item.PLACA == null ? "-" : item.PLACA.ToUpper();
                    cls.CACAMBA = item.CACAMBA == null ? "-" : item.CACAMBA.ToUpper();
                    cls.MOTORISTA = item.MOTORISTA == null ? "-" : item.MOTORISTA.ToUpper();
                    cls.TRANSPORTADORA = item.TRANPORTADORA == null ? "-" : item.TRANPORTADORA.ToUpper();
                    cls.PRODUTO = item.PRODUTO == null ? "-" : item.PRODUTO.ToUpper();
                    cls.CLIENTE = item.CLIENTE == null ? "-" : item.CLIENTE.ToUpper();



                    String dataEntradaConvertida = "-";
                    if (item.DATAENTRADA.ToString() != "" && item.DATAENTRADA.ToString() != "01/01/1753 00:00:00")
                    {
                        DateTime? dataEntrada = item.DATAENTRADA;
                        dataEntradaConvertida = dataEntrada?.ToString("dd/MM/yyyy HH:mm");
                    }

                    String dataSaidaConvertida = "-";
                    if (item.DATASAIDA.ToString() != "" && item.DATASAIDA.ToString() != "01/01/1753 00:00:00")
                    {
                        DateTime? dataSaida = item.DATASAIDA;
                        dataSaidaConvertida = dataSaida?.ToString("dd/MM/yyyy HH:mm");
                    }

                    cls.DATAENTRADA = dataEntradaConvertida;
                    cls.DATASAIDA = dataSaidaConvertida;

                    cls.PESOTARA = item.PESOTARA.ToString() == "" ? "-" : item.PESOTARA.ToString() + "kg";
                    cls.PESOENTRADA = item.PESOENTRADA.ToString() == "" ? "-" : item.PESOENTRADA.ToString() + "kg";
                    cls.PESOSAIDA = item.PESOSAIDA.ToString() == "" ? "-" : item.PESOSAIDA.ToString() + "kg";
                    cls.PESOLIQUIDO = item.PESOLIQUIDO.ToString() == "" ? "-" : item.PESOLIQUIDO.ToString() + "kg";

                    novaList.Add(cls);

                }


                Response.AddHeader("Refresh", "60");

                return View(novaList.ToPagedList(numeroPagina, tamanhoPagina));







            }catch( Exception e)
            {                
                return View("Error");
            }
        }



        public class relatorio
        {
            public int TICKET;
            public string BALANCEIRO;
            public string PLACA;
            public string CACAMBA;
            public string MOTORISTA;
            public string TRANSPORTADORA;
            public string PRODUTO;
            public string CLIENTE;
            public string DATAENTRADA;
            public string DATASAIDA;
            public string PESOTARA;
            public string PESOENTRADA;
            public string PESOSAIDA;
            public string PESOLIQUIDO;
        }


    }
}