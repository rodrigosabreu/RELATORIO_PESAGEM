using RELATORIO_PESAGEM.ModelsBALANCA;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace RELATORIO_PESAGEM.Controllers
{
    public class FechamentoPesagemController : Controller
    {

        private BALANCATRANSBORDOEntities db = new BALANCATRANSBORDOEntities();


        private String ConverteDataParaIngles(String data)
        {

            string[] array = data.Split('/');

            string resp = array[1] + "-" + array[0] + "-" + array[2];

            return resp;
        }



        // GET: FechamentoPesagem
        public ActionResult Index(String data1, String data2)
        {

            if (Session["sessao"] == null)
            {

                return RedirectToAction("Login", "Account");

            }
            else
            {
                if (Session["cargo"].ToString() != "administrador")
                {
                    return RedirectToAction("Login", "Account");
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





                DateTime date = DateTime.Now;
                DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                String primeiroDiaMes = firstDayOfMonth.ToString("dd/MM/yyyy");


                DateTime data = DateTime.Now;
                String hoje = data.ToString("dd/MM/yyyy");



                ViewBag.data1 = primeiroDiaMes;
                ViewBag.data2 = hoje;



                data1 = ConverteDataParaIngles(primeiroDiaMes);
                data2 = ConverteDataParaIngles(hoje);
            }

            data1 = data1 + " 00:00:00.000";
            data2 = data2 + " 23:59:59.999";

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);

            DateTime dt1 = Convert.ToDateTime(data1);
            DateTime dt2 = Convert.ToDateTime(data2);






            var result = db.RelatorioFechamento(dt1, dt2).ToList();


            

            return View(result);
        }




        // GET: FechamentoPesagem
        public ActionResult Excel(int? clienteID, String data1, String data2)
        {

            data1 = ConverteDataParaIngles(data1);
            data2 = ConverteDataParaIngles(data2);

            data1 = data1 + " 00:00:00.000";
            data2 = data2 + " 23:59:59.999";

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);

            DateTime dt1 = Convert.ToDateTime(data1);
            DateTime dt2 = Convert.ToDateTime(data2);






            int?[] clientes_atrelados;
            int i = 0;

            string nome_cli = "";

            if (string.IsNullOrEmpty(clienteID.ToString()))
            {


                nome_cli = "Pesagem_Geral";

                var list = (from cu in db.CLIENTEUSUARIO
                            select new
                            {
                                IDCLIENTE = cu.idcliente
                            }).ToList();


                clientes_atrelados = new int?[list.Count];

                foreach (var item in list)
                {
                    clientes_atrelados[i] = item.IDCLIENTE;
                    i = i + 1;
                }



                if (Session["cargo"].ToString() == "administrador")
                {

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






            }
            else
            {
                clientes_atrelados = new int?[1];
                clientes_atrelados[i] = clienteID;
                


                int id = Convert.ToInt32(clienteID);

                var list_cliente = (from c in db.CLIENTE
                                    where c.ID == id
                                    select new
                                    {
                                        nome = c.NOME
                                    }).ToList();

                nome_cli = "Pesagem_" + list_cliente[0].nome;

            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR", true);

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

                             DATA_ENTRADA = p.DATAPESAGEM,
                             DATA_SAIDA = p.DATAPESAGEM2,

                             //DATA_SAIDA = p.DATAPESAGEM2.ToString() == "01/01/1753 00:00:00" ? "-" : p.DATAPESAGEM2,

                             PESO_TARA_KG = p.PESOTARA,
                             PESO_ENTRADA_KG = p.PESOTOTAL,
                             PESO_SAIDA_KG = p.PESOTOTAL2,
                             PESO_LIQUIDO_KG = p.PESOLIQUIDO,

                             TIPO_PESAGEM = p.TIPOPESAGEM,
                             STATUS = p.STATUS,

                             

                         })
                             .OrderByDescending(p => p.TICKET);





            /*GridView gv = new GridView();
            gv.DataSource = lista.ToList();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Marklist.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();*/






            // Step 1 - get the data from database
            var data = lista.ToList();

            // instantiate the GridView control from System.Web.UI.WebControls namespace
            // set the data source
            GridView gridview = new GridView();
            gridview.DataSource = data;
            gridview.DataBind();

            // Clear all the content from the current response
            Response.ClearContent();
            Response.Buffer = true;
            // set the header
            Response.AddHeader("content-disposition", "attachment;filename = "+nome_cli+".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            // create HtmlTextWriter object with StringWriter
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    // render the GridView to the HtmlTextWriter
                    gridview.RenderControl(htw);
                    // Output the GridView content saved into StringWriter
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }







            return RedirectToAction("FechamentoPesagem", "Index");
        }


    



    }
}