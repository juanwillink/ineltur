using System;
using System.ComponentModel.DataAnnotations;

namespace ArgentinahtlCommon.DTO.NPS
{
	public class PagoNPSDTO
	{
		public long IdPagoNPS { get; set; }
		public string IdTransaccion { get; set; }
		public string CodigoReserva { get; set; }
		public long ReservationId { get; set; }
		public int IdEstadoNPS { get; set; }
		public string MotivoEstado { get; set; }
		public long? IdTransaccionNPS { get; set; }
		public DateTime? FechaGeneracion { get; set; }
		public string CodigoBarra { get; set; }
		public int? NPSCantCuotas { get; set; }
		public string Referencia { get; set; }
		public int? DiasComprobanteVencido { get; set; }
		public int? ResponseCod { get; set; }
		public string ResponseMsg { get; set; }
		public string ResponseExtended { get; set; }
	}
}
