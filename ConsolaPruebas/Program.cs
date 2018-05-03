using System;
using Ineltur.Datos.Entidades;
using System.Configuration;
using System.Linq;
using ArgentinahtlBLL;
using ArgentinahtlCommon.DTO.NPS;
using ArgentinahtlCommon;

namespace ConsolaPruebas
{
	class Program
	{
		static void Main(string[] args)
		{
			ContextoBLL db = new ContextoBLL();

			//using (var db = new WebServiceDataContext(ConfigurationManager.ConnectionStrings["ArgentinahtlConnectionString"].ConnectionString))
			//{
				PAGO_NPS p = new PAGO_NPS();
				p.ReservationId = 2;
				p.IdEstadoNPS = 1;
				p.FechaGeneracion = DateTime.Now;
				db.Context.PAGO_NPS.InsertOnSubmit(p);

				db.Context.SubmitChanges();

				Console.WriteLine(p);
			//}

			//NpsBLL npsBLL = new NpsBLL();
			//var dto = npsBLL.GuardarPagoNPS(new PagoNPSDTO
			//{
			//	//IdPagoNPS = long.Parse("2"),
			//	ReservationId = long.Parse("3"),
			//	IdEstadoNPS = (int)EstadoNPS.Iniciada,
			//	FechaGeneracion = DateTime.Now
			//});

			//Console.WriteLine(dto.IdPagoNPS);
		}
	}

	public class ContextoBLL
	{
		public WebServiceDataContext Context
		{
			get
			{
				return new WebServiceDataContext(ConfigurationManager.ConnectionStrings["ArgentinahtlConnectionString"].ConnectionString);

			}

		}

	}
}
