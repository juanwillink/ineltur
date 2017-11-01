using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgentinahtlBLL.NPS;
using ArgentinahtlCommon;

namespace ArgentinahtlBLL
{
    public class NpsBLL
    {
		//private LogBLL log;
		//private CommunicationMsgLogDTO dtoComm;
		public NpsBLL()
		{
			//log = new LogBLL();
			//dtoComm = new CommunicationMsgLogDTO();
		}

		public RespuestaPayOnline3pDTO ObtenerDatosPostTC(long idPago)
		{

			Pago pago = Context.Pago.First(c => c.IdPago == idPago);

			RequerimientoPayOnline3pDTO dtoRequest = new RequerimientoPayOnline3pDTO();
			NPSConfigSection NpsConfig = Globals.ConfiguracionNPS;
			//datos generales------
			string urlNps = NpsConfig.Url;
			dtoRequest.Version = NpsConfig.Version;
			dtoRequest.TxSource = NpsConfig.TxSource;
			dtoRequest.FrmLanguage = NpsConfig.FrmLanguage;
			dtoRequest.Country = NpsConfig.Country;
			dtoRequest.ReturnURL = NpsConfig.ReturnURL;
			dtoRequest.FrmBackButtonURL = NpsConfig.FrmBackButtonURL;
			//datos de la empresa recaudadora en NPS------
			dtoRequest.MerchantId = NpsConfig.NPSMerchantID;
			dtoRequest.SecretKey = NpsConfig.NPSSecretKey;
			//datos de la transacción------
			dtoRequest.MerchOrderId = pago.Comprobante.IdComprobanteCanal.ToString();
			dtoRequest.MerchTxRef = pago.IdPago.ToString();
			dtoRequest.Amount = pago.Importe;
			dtoRequest.NumPayments = pago.PagoNPS.NPSCantCuotas ?? 1;
			//dtoRequest.Plan = null; es opcional, no usado por el momento
			dtoRequest.Currency = pago.Comprobante.Moneda.CodigoNPS;
			dtoRequest.Product = Convert.ToInt32(empMedio.MedioPago.CodigoNPS);
			dtoRequest.CustomerId = pago.Comprobante.Ordenante != null ? pago.Comprobante.Ordenante.TipoDocumento.Codigo + pago.Comprobante.Ordenante.NroDocumento : string.Empty; //dato a ingresar y persistir en PagoNPS
			dtoRequest.CustomerMail = pago.Comprobante.Ordenante != null ? pago.Comprobante.Ordenante.Email : string.Empty; //dato a ingresar y persistir en PagoNPS
																															//dtoRequest.PurchaseDescription = pago.PagoNPS.Descripcion; //dato a ingresar y persistir en PagoNPS
			dtoRequest.PosDateTime = DateTime.Now;
			dtoRequest.MerchantMail = NpsConfig.MerchantMail;

			if (empMedio.NPSVerifiedByVisa != null && empMedio.NPSVerifiedByVisa == true && pago.Importe >= (empMedio.NPSVerifiedByVisaMonto ?? 0))
				dtoRequest.TresDSecureAction = NPSVerifiedByVisa.AutorizarSiAutenticacion3dSecureOk; //si el dato no se envía en el POST por defecto NPS asume 0 (AutorizarSinAutenticacion)

			//dtoComm.IdPago = idPago;
			//dtoComm.FechaSolicitud = DateTime.Now;
			//dtoComm.MensajeSolicitud = dtoRequest;
			//dtoComm.NombreServicio = "PayOnline3p";
			//dtoComm.IdComprobante = pago.Comprobante.IdComprobanteCanal;
			//dtoComm.IdMedioPago = pago.IdMedioPago;
			//dtoComm.IdRequest = pago.IdRequest != null ? pago.IdRequest.ToString() : null;

			var rpta = new ServiciosNPS(urlNps).PayOnLine_3p(dtoRequest);

			//dtoComm.MensajeRespuesta = rpta;
			//dtoComm.FechaRespuesta = DateTime.Now;
			//dtoComm.IdTipoServicio = (int)TipoServicioEnum.PagoConTarjetaNPS;

			//log.RegistrarMensajeIntegracion(dtoComm);

			//para no mostrar mensajes de error al cliente
			if (!string.IsNullOrEmpty(rpta.ErrorMessage))
				rpta.ErrorMessage = rpta.ErrorMessage.Contains("INELPOL3P") ? $"La solicitud no puede ser procesada en este momento. ID: {idPago}" : rpta.ErrorMessage;

			return rpta;


		}
		
		public string ActualizarEstadoPago(long idPago, string idRequest)
		{
			string respuesta = string.Empty;

			Pago pago = Context.Pago.First(c => c.IdPago == idPago);

			RequerimientoSimpleQueryTxDTO dtoRequest = new RequerimientoSimpleQueryTxDTO();
			NPSConfigSection NpsConfig = Globals.ConfiguracionNPS;
			//datos generales------
			string urlNps = Context.Configuracion.First(c => c.IdConfiguracion == (int)ConfiguracionEnum.NPSURL).Valor;
			dtoRequest.Version = NpsConfig.Version;
			dtoRequest.MerchantId = NpsConfig.NPSMerchantID;
			dtoRequest.SecretKey = NpsConfig.NPSSecretKey;
			dtoRequest.QueryCriteria = NpsConfig.QueryCriteriaMerchTxRef;
			dtoRequest.QueryCriteriaId = idPago.ToString(); //idPago es el merchTxRef;
			dtoRequest.PosDateTime = DateTime.Now;

			//dtoComm.IdPago = idPago;
			//dtoComm.FechaSolicitud = DateTime.Now;
			//dtoComm.MensajeSolicitud = dtoRequest;
			//dtoComm.NombreServicio = "SimpleQueryTx";
			//dtoComm.IdComprobante = pago.Comprobante.IdComprobanteCanal;
			//dtoComm.IdMedioPago = pago.IdMedioPago;
			//dtoComm.IdRequest = idRequest;

			RespuestaSimpleQueryTxDTO dtoResponse = new ServiciosNPS(urlNps).SimpleQueryTx(dtoRequest);

			//dtoComm.MensajeRespuesta = dtoResponse;
			//dtoComm.FechaRespuesta = DateTime.Now;
			//dtoComm.IdTipoServicio = (int)TipoServicioEnum.ConsultarEstadoTransaccionNPS;

			//log.RegistrarMensajeIntegracion(dtoComm);

			if (dtoResponse.ErrorMessage == null)
			{
				//log.RegistrarMensajeIntegracion(idPago, dtoResponse.Transaction);

				using (var tx = Context.Database.BeginTransaction())
				{
					//si la operación de consulta fue exitosa y encontró una transacción
					if (dtoResponse.ResponseCod.ToString() == CodigoRespuestaSimpleQueryTxNPS.Exitosa)
					{
						pago.PagoNPS.IdPago = idPago;
						pago.PagoNPS.IdEstadoNPS = ObtenerIdEstadoNPS(dtoResponse.Transaction.ResponseCod);
						pago.PagoNPS.MotivoEstado = dtoResponse.Transaction.ResponseMsg;
						pago.PagoNPS.MotivoEstado += dtoResponse.Transaction.ResponseExtended != null ? string.Format(" {0}", dtoResponse.Transaction.ResponseExtended) : "";
						pago.PagoNPS.IdTransaccionNPS = dtoResponse.Transaction.TransactionId;

						pago.IdEstadoPago = ObtenerIdEstadoPago(pago.PagoNPS.IdEstadoNPS);

						if (pago.PagoNPS.IdEstadoNPS != (int)EstadoNPSEnum.AprobadaAutorizada
							&& pago.PagoNPS.IdEstadoNPS != (int)EstadoNPSEnum.PendienteEnComprador_CashPayment)
							respuesta = pago.PagoNPS.MotivoEstado;

					}
					else
					{
						//falta definir en qué estado quedará si no se encuentra ninguna trx con SimpleQueryTx
						//actualmente queda en estado Iniciada
						respuesta = dtoResponse.ResponseMsg + " - " + dtoResponse.ResponseExtended;
					}

					Context.SaveChanges();
					tx.Commit();
				}
			}
			else
			{
				respuesta = dtoResponse.ErrorMessage;
			}

			return respuesta;

		}

		public void ActualizarPagosPendientesNPS()
		{
			string idRequest = Guid.NewGuid().ToString();

			List<Pago> pagos = Context.Pago.Where(p => (p.EstadoPago.IdEstadoPago == (int)EstadoPagoEnum.Procesando || p.EstadoPago.IdEstadoPago == (int)EstadoPagoEnum.Pendiente)
													&& (p.MedioPago.IdPadre == (int)MedioPagoEnum.TarjetaDeCreditoNPS || p.MedioPago.IdPadre == (int)MedioPagoEnum.PagoPorCajaNPS)).ToList();

			#region Log
			//CommunicationMsgLogDTO dtolog = new CommunicationMsgLogDTO();
			//dtolog.IdRequest = idRequest;
			//dtolog.FechaSolicitud = DateTime.Now;
			//dtolog.MensajeSolicitud = string.Format("Pagos a actualizar: {0}", pagos.Count);
			//dtolog.NombreServicio = WinServices.ActualizarPagosPendientesNps.ToString();
			//dtolog.IdMedioPago = (int)MedioPagoEnum.PagoPorCajaNPS;
			#endregion

			foreach (Pago p in pagos)
			{
				Tracker.WriteTrace($"ActualizarPagosPendientesNPS idPago: {p.IdPago}");

				string msg = string.Empty;
				var rpta = ActualizarEstadoPago(p.IdPago, idRequest);

				if (rpta == string.Empty)
				{
					msg = "OK";

					Tracker.WriteTrace(msg);
					//dtolog.MensajeRespuesta += string.Format("Pago: {0}, Rpta NPS: {1} ", p.IdPago, msg);
				}
				else
				{
					Tracker.WriteTrace(string.Format("Error: {0}", rpta));
					//dtolog.MensajeRespuesta += string.Format("Pago: {0}, Rpta NPS: {1} ", p.IdPago, rpta);
				}
				
			}

			#region Log
			//dtolog.FechaRespuesta = DateTime.Now;
			//dtolog.IdTipoServicio = (int)TipoServicioEnum.ActualizarPagosPendientesNps;
			//log.RegistrarMensajeIntegracion(dtolog);
			#endregion

		}

		private int ObtenerIdEstadoPago(int idEstadoNPS)
		{
			try
			{
				using (var cxt = new Entities())
				{
					return cxt.EstadoNPSReca.Single(x => x.IdEstadoNPS == idEstadoNPS).IdEstadoPago;
				}
			}
			catch (Exception)
			{
				throw;
			}

		}

		private int ObtenerIdEstadoNPS(int codigoEstadoNPS)
		{
			try
			{
				using (var cxt = new Entities())
				{
					return cxt.EstadoNPS.Single(x => x.Codigo == codigoEstadoNPS).IdEstadoNPS;
				}
			}
			catch (Exception)
			{
				throw;
			}

		}


	}
}
