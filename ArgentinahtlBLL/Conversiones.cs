using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ineltur.Datos.Entidades;
using ArgentinahtlCommon.DTO.NPS;


namespace ArgentinahtlBLL
{
	public static class Conversiones
	{
		public static PagoNPSDTO ToDTO(this PAGO_NPS entity)
		{
			return new PagoNPSDTO
			{
				IdPagoNPS = entity.IdPagoNPS,
				IdTransaccion = entity.IdTransaccion.HasValue ? entity.IdTransaccion.ToString() : string.Empty,
				CodigoReserva = entity.CodigoReserva.ToString(),
				ReservationId = entity.ReservationId,
				IdEstadoNPS = entity.IdEstadoNPS,
				MotivoEstado = entity.MotivoEstado,
				IdTransaccionNPS = entity.IdTransaccionNPS,
				FechaGeneracion = entity.FechaGeneracion,
				CodigoBarra = entity.CodigoBarra,
				NPSCantCuotas = entity.NPSCantCuotas,
				Referencia = entity.Referencia,
				DiasComprobanteVencido = entity.DiasComprobanteVencido,
				ResponseCod = entity.ResponseCod,
				ResponseMsg = entity.ResponseMsg,
				ResponseExtended = entity.ResponseExtended
			};

		}

		public static PAGO_NPS ToEntity(this PagoNPSDTO dto)
		{
			return new PAGO_NPS
			{
				IdTransaccion = !string.IsNullOrEmpty(dto.IdTransaccion) ? new Guid(dto.IdTransaccion) : (Guid?)null,
				CodigoReserva = !string.IsNullOrEmpty(dto.CodigoReserva) ? int.Parse(dto.CodigoReserva) : (int?)null,
				ReservationId = dto.ReservationId,
				IdEstadoNPS = dto.IdEstadoNPS,
				//entity.ESTADO_PAGO_NPS = new NpsBLL().Context.ESTADO_PAGO_NPS.FirstOrDefault(e => e.IdEstadoNPS == dto.IdEstadoNPS);
				MotivoEstado = dto.MotivoEstado,
				IdTransaccionNPS = dto.IdTransaccionNPS,
				FechaGeneracion = dto.FechaGeneracion.HasValue ? dto.FechaGeneracion.Value : DateTime.MinValue,
				CodigoBarra = dto.CodigoBarra,
				NPSCantCuotas = dto.NPSCantCuotas,
				Referencia = dto.Referencia,
				DiasComprobanteVencido = dto.DiasComprobanteVencido,
				ResponseCod = dto.ResponseCod,
				ResponseMsg = dto.ResponseMsg,
				ResponseExtended = dto.ResponseExtended
			};
	
		}
	}
}
