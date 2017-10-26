﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RELATORIO_PESAGEM.ModelsBALANCA
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class BALANCATRANSBORDOEntities : DbContext
    {
        public BALANCATRANSBORDOEntities()
            : base("name=BALANCATRANSBORDOEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CACAMBA> CACAMBA { get; set; }
        public virtual DbSet<CLIENTE> CLIENTE { get; set; }
        public virtual DbSet<PRODUTO> PRODUTO { get; set; }
        public virtual DbSet<TRANSPORTADORA> TRANSPORTADORA { get; set; }
        public virtual DbSet<USUARIO> USUARIO { get; set; }
        public virtual DbSet<CLIENTEUSUARIO> CLIENTEUSUARIO { get; set; }
        public virtual DbSet<PESAGEM> PESAGEM { get; set; }
        public virtual DbSet<VEICULO> VEICULO { get; set; }
    
        public virtual ObjectResult<RelatorioFechamento_Result> RelatorioFechamento(Nullable<System.DateTime> dateFirst, Nullable<System.DateTime> dateLast)
        {
            var dateFirstParameter = dateFirst.HasValue ?
                new ObjectParameter("DateFirst", dateFirst) :
                new ObjectParameter("DateFirst", typeof(System.DateTime));
    
            var dateLastParameter = dateLast.HasValue ?
                new ObjectParameter("DateLast", dateLast) :
                new ObjectParameter("DateLast", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<RelatorioFechamento_Result>("RelatorioFechamento", dateFirstParameter, dateLastParameter);
        }
    }
}
