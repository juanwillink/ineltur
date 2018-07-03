using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgentinahtlBLL.NPS;
using ArgentinahtlCommon;
using ArgentinahtlCommon.DTO.NPS;
using Ineltur.Datos.Entidades;

namespace ArgentinahtlBLL
{
    public class NpsBLL
    {
		//private LogBLL log;
		//private CommunicationMsgLogDTO dtoLog;
		public NpsBLL()
		{
			//log = new LogBLL();
			//dtoLog = new CommunicationMsgLogDTO();
		}

		public RespuestaAuthorize3pDTO ObtenerDatosPostTC(string idPagoNPS, string codigoReserva, float importe, int cantCuotas, string codigoMoneda, string codigoTarjeta, bool verifiedByVisa, float verifiedByVisaMonto, string documentoPasajero, string mailPasajero)
		{

			RequerimientoAuthorize3pDTO dtoRequest = new RequerimientoAuthorize3pDTO();
			//datos generales------
			string urlNps = NPSConfiguracion.Url;
			dtoRequest.Version = NPSConfiguracion.Version;
			dtoRequest.TxSource = NPSConfiguracion.TxSource;
			dtoRequest.FrmLanguage = NPSConfiguracion.FrmLanguage;
			dtoRequest.Country = NPSConfiguracion.Country;
			dtoRequest.ReturnURL = $"{Consts.urlRaizSitio}{NPSConfiguracion.ReturnURL}";
			dtoRequest.FrmBackButtonURL = $"{Consts.urlRaizSitio}{NPSConfiguracion.FrmBackButtonURL}";

			dtoRequest.MerchantId = NPSConfiguracion.MerchantID;
			dtoRequest.SecretKey = NPSConfiguracion.NPSSecretKey;
			//datos de la transacción------
			dtoRequest.MerchOrderId = codigoReserva;
			dtoRequest.MerchTxRef = idPagoNPS;
			dtoRequest.Amount = importe;
			dtoRequest.NumPayments = cantCuotas;
			//dtoRequest.Plan = null; es opcional, no usado por el momento
			dtoRequest.Currency = codigoMoneda;
			dtoRequest.Product = codigoTarjeta;
			dtoRequest.CustomerId = documentoPasajero != null ? documentoPasajero : string.Empty; //dato a ingresar y persistir en PagoNPS
			dtoRequest.CustomerMail = mailPasajero != null ? mailPasajero : string.Empty; //dato a ingresar y persistir en PagoNPS
																															//dtoRequest.PurchaseDescription = pago.PagoNPS.Descripcion; //dato a ingresar y persistir en PagoNPS
			dtoRequest.PosDateTime = DateTime.Now;
			dtoRequest.MerchantMail = NPSConfiguracion.MerchantMail;

			if (verifiedByVisa && importe >= verifiedByVisaMonto)
				dtoRequest.TresDSecureAction = NPSVerifiedByVisa.AutorizarSiAutenticacion3dSecureOk; //si este dato no se envía en el POST por defecto NPS asume 0 (AutorizarSinAutenticacion)

			//dtoLog.IdTransaccion = idTransaccion;
			//dtoLog.FechaSolicitud = DateTime.Now;
			//dtoLog.MensajeSolicitud = dtoRequest;
			//dtoLog.NombreServicio = "NPS-PayOnline3p";
			//dtoLog.IdRequest = pago.IdRequest != null ? pago.IdRequest.ToString() : null;

			var rpta = new ServiciosNPS(urlNps).Authorize_3p(dtoRequest);

			//dtoLog.MensajeRespuesta = rpta;
			//dtoLog.FechaRespuesta = DateTime.Now;

			//log.RegistrarLog(dtoLog);

			//para no mostrar mensajes de error al cliente
			if (!string.IsNullOrEmpty(rpta.ErrorMessage))
				rpta.ErrorMessage = rpta.ErrorMessage.Contains("INELPOL3P") ? $"La solicitud no puede ser procesada en este momento. ID: {idPagoNPS}" : rpta.ErrorMessage;

			return rpta;


		}
		
		public string ActualizarEstadoPago(string idPagoNPS)
		{
			string respuesta = string.Empty;

			RequerimientoSimpleQueryTxDTO dtoRequest = new RequerimientoSimpleQueryTxDTO();

			//datos generales------
			string urlNps = NPSConfiguracion.Url;
			dtoRequest.Version = NPSConfiguracion.Version;
			dtoRequest.MerchantId = NPSConfiguracion.MerchantID;
			dtoRequest.SecretKey = NPSConfiguracion.NPSSecretKey;
			dtoRequest.QueryCriteria = NPSConfiguracion.QueryCriteriaMerchTxRef;
			dtoRequest.QueryCriteriaId = idPagoNPS; //idPago es el merchTxRef;
			dtoRequest.PosDateTime = DateTime.Now;

			//dtoLog.IdTransaccion = idTransaccion;
			//dtoLog.FechaSolicitud = DateTime.Now;
			//dtoLog.MensajeSolicitud = dtoRequest;
			//dtoLog.NombreServicio = "NPS-SimpleQueryTx";
			//dtoLog.IdRequest = idRequest;

			RespuestaSimpleQueryTxDTO dtoResponse = new ServiciosNPS(urlNps).SimpleQueryTx(dtoRequest);

			//dtoLog.MensajeRespuesta = dtoResponse;
			//dtoLog.FechaRespuesta = DateTime.Now;

			//log.RegistrarLog(dtoLog);

			if (dtoResponse.ErrorMessage == null)
			{
				using (var Context = new WebServiceDataContext())
				{
					//log.RegistrarLog(idPago, dtoResponse.Transaction);
					//using (var tx = new WebServiceDataContext().Connection.BeginTransaction())
					//{
						PAGO_NPS pagoNPS = Context.PAGO_NPS.First(c => c.IdPagoNPS == int.Parse(idPagoNPS));
						//si la operación de consulta fue exitosa y encontró una transacción
						if (dtoResponse.ResponseCod.ToString() == RespuestaSimpleQueryTxNPS.Exitosa)
						{
							//pagoNPS.IdPago = idPago;
							pagoNPS.IdEstadoNPS = ObtenerIdEstadoNPS(dtoResponse.Transaction.ResponseCod);
							pagoNPS.MotivoEstado = dtoResponse.Transaction.ResponseMsg;
							pagoNPS.MotivoEstado += dtoResponse.Transaction.ResponseExtended != null ? string.Format(" {0}", dtoResponse.Transaction.ResponseExtended) : "";
							pagoNPS.IdTransaccionNPS = dtoResponse.Transaction.TransactionId;
							//pagoNPS.IdTransaccion = !string.IsNullOrEmpty(idTransaccion) ? new Guid(idTransaccion) : (Guid?)null;
							pagoNPS.ResponseCod = dtoResponse.ResponseCod;
							pagoNPS.ResponseMsg = dtoResponse.ResponseMsg;
							pagoNPS.ResponseExtended = dtoResponse.ResponseExtended;

							//hay que ver si el estado de la transacción no se cambia en otra parte
							//pagoNPS.Transaccion.EstadoPago = ObtenerIdEstadoTransaccion(pagoNPS.IdEstadoNPS);

							if (pagoNPS.IdEstadoNPS != (int)EstadoNPS.AprobadaAutorizada
								&& pagoNPS.IdEstadoNPS != (int)EstadoNPS.PendienteEnComprador_CashPayment)
								respuesta = pagoNPS.MotivoEstado;
						}
						else
						{
							//falta definir en qué estado quedará si no se encuentra ninguna trx con SimpleQueryTx
							//actualmente queda en estado Iniciada
							respuesta = dtoResponse.ResponseMsg + " - " + dtoResponse.ResponseExtended;
						}

						Context.SubmitChanges();
						//tx.Commit();
					//}
				}
			}
			else
			{
				respuesta = dtoResponse.ErrorMessage;
			}

			return respuesta;

		}

		public string CapturarTrx(string idPagoNPS, string idTransaccionNPS, string idTransaccion, float importe)
		{
			string respuesta = string.Empty;

			RequerimientoCapture3pDTO dtoRequest = new RequerimientoCapture3pDTO();

			//datos generales------
			string urlNps = NPSConfiguracion.Url;
			dtoRequest.Version = NPSConfiguracion.Version;
			dtoRequest.TxSource = NPSConfiguracion.TxSource;
			dtoRequest.MerchantId = NPSConfiguracion.MerchantID;
			dtoRequest.SecretKey = NPSConfiguracion.NPSSecretKey;
			dtoRequest.TransactionId_Orig = idTransaccionNPS;
			dtoRequest.AmountToCapture = (importe == 0 ? null : Math.Truncate(importe * 100).ToString());
			dtoRequest.MerchTxRef = $"{idPagoNPS}-1"; //idPago es el merchTxRef;
			dtoRequest.PosDateTime = DateTime.Now;

			//dtoLog.IdTransaccion = idTransaccion;
			//dtoLog.FechaSolicitud = DateTime.Now;
			//dtoLog.MensajeSolicitud = dtoRequest;
			//dtoLog.NombreServicio = "NPS-SimpleQueryTx";
			//dtoLog.IdRequest = idRequest;

			RespuestaCaptureDTO dtoResponse = new ServiciosNPS(urlNps).Capture3p(dtoRequest);

			//dtoLog.MensajeRespuesta = dtoResponse;
			//dtoLog.FechaRespuesta = DateTime.Now;

			//log.RegistrarLog(dtoLog);

			if (dtoResponse.ErrorMessage == null)
			{
				using (var Context = new WebServiceDataContext())
				{
					//log.RegistrarLog(idPago, dtoResponse.Transaction);
					//using (var tx = new WebServiceDataContext().Connection.BeginTransaction())
					//{
					PAGO_NPS pagoNPS = Context.PAGO_NPS.First(c => c.IdPagoNPS == int.Parse(idPagoNPS));
					//si la operación de consulta fue exitosa y encontró una transacción
					if (dtoResponse.ResponseCod.ToString() == RespuestaSolicitudCapturaNPS.Exitosa)
					{
						//pagoNPS.IdPago = idPago;
						pagoNPS.IdEstadoNPS = ObtenerIdEstadoNPS(dtoResponse.ResponseCod);
						pagoNPS.MotivoEstado = dtoResponse.ResponseMsg;
						pagoNPS.MotivoEstado += dtoResponse.ResponseExtended != null ? string.Format(" {0}", dtoResponse.ResponseExtended) : "";
						pagoNPS.IdTransaccionNPS = dtoResponse.TransactionId;
						pagoNPS.IdTransaccion = !string.IsNullOrEmpty(idTransaccion) ? new Guid(idTransaccion) : (Guid?)null;
						pagoNPS.ResponseCod = dtoResponse.ResponseCod;
						pagoNPS.ResponseMsg = dtoResponse.ResponseMsg;
						pagoNPS.ResponseExtended = dtoResponse.ResponseExtended;

						//hay que ver si el estado de la transacción no se cambia en otra parte
						//pagoNPS.Transaccion.EstadoPago = ObtenerIdEstadoTransaccion(pagoNPS.IdEstadoNPS);

						if (pagoNPS.IdEstadoNPS != (int)EstadoNPS.AprobadaAutorizada
							&& pagoNPS.IdEstadoNPS != (int)EstadoNPS.PendienteEnComprador_CashPayment)
							respuesta = pagoNPS.MotivoEstado;
					}
					else
					{
						//falta definir en qué estado quedará si no se encuentra ninguna trx con SimpleQueryTx
						//actualmente queda en estado Iniciada
						respuesta = dtoResponse.ResponseMsg + " - " + dtoResponse.ResponseExtended;
					}

					Context.SubmitChanges();
					//tx.Commit();
					//}
				}
			}
			else
			{
				respuesta = dtoResponse.ErrorMessage;
			}

			return respuesta;

		}

		//public void ActualizarPagosPendientesNPS()
		//{
		//	string idRequest = Guid.NewGuid().ToString();

		//	List<PAGO_NPS> pagos = Context.PAGO_NPS.Where(p => (p.EstadoPago.IdEstadoPago == (int)EstadoPago.Procesando || p.EstadoPago.IdEstadoPago == (int)EstadoPagoEnum.Pendiente)
		//											&& (p.MedioPago.IdPadre == (int)MedioPago.TarjetaDeCreditoNPS || p.MedioPago.IdPadre == (int)MedioPagoEnum.PagoPorCajaNPS)).ToList();

		//	#region Log
		//	//CommunicationMsgLogDTO dtolog = new CommunicationMsgLogDTO();
		//	//dtolog.IdRequest = idRequest;
		//	//dtolog.FechaSolicitud = DateTime.Now;
		//	//dtolog.MensajeSolicitud = string.Format("Pagos a actualizar: {0}", pagos.Count);
		//	//dtolog.NombreServicio = WinServices.ActualizarPagosPendientesNps.ToString();
		//	//dtolog.IdMedioPago = (int)MedioPagoEnum.PagoPorCajaNPS;
		//	#endregion

		//	foreach (Pago p in pagos)
		//	{
		//		Tracker.WriteTrace($"ActualizarPagosPendientesNPS idPago: {p.IdPago}");

		//		string msg = string.Empty;
		//		var rpta = ActualizarEstadoPago(p.IdPago, idRequest);

		//		if (rpta == string.Empty)
		//		{
		//			msg = "OK";

		//			Tracker.WriteTrace(msg);
		//			//dtolog.MensajeRespuesta += string.Format("Pago: {0}, Rpta NPS: {1} ", p.IdPago, msg);
		//		}
		//		else
		//		{
		//			Tracker.WriteTrace(string.Format("Error: {0}", rpta));
		//			//dtolog.MensajeRespuesta += string.Format("Pago: {0}, Rpta NPS: {1} ", p.IdPago, rpta);
		//		}

		//	}

		//	#region Log
		//	//dtolog.FechaRespuesta = DateTime.Now;
		//	//dtolog.IdTipoServicio = (int)TipoServicioEnum.ActualizarPagosPendientesNps;
		//	//log.RegistrarLog(dtolog);
		//	#endregion

		//}

		//private int ObtenerIdEstadoPago(int idEstadoNPS)
		//{
		//	try
		//	{
		//		return Context.ESTADO_PAGO_NPS.Single(x => x.IdEstadoNPS == idEstadoNPS).IdEstadoPago;
		//	}
		//	catch (Exception)
		//	{
		//		throw;
		//	}

		//}

		private int ObtenerIdEstadoNPS(int codigoEstadoNPS)
		{
			using (var Context = new WebServiceDataContext())
			{
				return Context.ESTADO_PAGO_NPS.Single(x => x.Codigo == codigoEstadoNPS).IdEstadoNPS;
			}

		}

		public PagoNPSDTO GuardarPagoNPS(PagoNPSDTO dto)
		{
			PAGO_NPS pagonps;

			try
			{
				using (var Context = new WebServiceDataContext())
				{
					if (dto.IdPagoNPS > 0)
					{
						pagonps = Context.PAGO_NPS.First(k => k.IdPagoNPS == dto.IdPagoNPS);
						pagonps.IdEstadoNPS = dto.IdEstadoNPS;
						pagonps.IdTransaccion = !string.IsNullOrEmpty(dto.IdTransaccion) ? new Guid(dto.IdTransaccion) : (Guid?)null;
						pagonps.CodigoReserva = !string.IsNullOrEmpty(dto.CodigoReserva) ? int.Parse(dto.CodigoReserva) : (int?)null;
						pagonps.ReservationId = dto.ReservationId;
						pagonps.MotivoEstado = dto.MotivoEstado;
						pagonps.IdTransaccionNPS = dto.IdTransaccionNPS;
						pagonps.FechaGeneracion = dto.FechaGeneracion.HasValue ? dto.FechaGeneracion.Value : DateTime.MinValue;
						pagonps.CodigoBarra = dto.CodigoBarra;
						pagonps.NPSCantCuotas = dto.NPSCantCuotas;
						pagonps.Referencia = dto.Referencia;
						pagonps.DiasComprobanteVencido = dto.DiasComprobanteVencido;

						Context.SubmitChanges();

					}
					else
					{
						pagonps = dto.ToEntity();
						Context.PAGO_NPS.InsertOnSubmit(pagonps);

						Context.SubmitChanges();
						pagonps = Context.PAGO_NPS.Where(p => p.ReservationId == dto.ReservationId).OrderByDescending(p => p.IdPagoNPS).FirstOrDefault();
					}

					return pagonps.ToDTO();
				}
			}
			catch (Exception e)
			{
				Tracker.WriteTrace(e.Message);
				return null;
			}
		}


	}
}
