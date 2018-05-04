using System.Collections.Generic;
using System.Web.Mvc;
using System.Globalization;
using System.Linq;
using CheckArgentina.Models;
using CheckArgentina.Commons;
using CheckArgentina.Managers;
using System;
using System.Net;
using System.Net.Mail;
using ArgentinahtlBLL;
using ArgentinahtlCommon.DTO.NPS;
using ArgentinahtlCommon;


namespace CheckArgentina.Controllers
{
    public class PaymentController : Controller
    {
        public ActionResult Confirmation(ReservationModel reservation)
        {
            var manager = new Manager(SessionData.SearchType);

            reservation.Vacancies = reservation.Vacancies;

            reservation = manager.ConfirmAvailability(reservation, SessionData.UserCredential);

            reservation.Countries = SessionData.Countries;

            reservation.PaymentMethods = SessionData.PaymentMethods;

            reservation.DiasCancelacionCargo = manager.GetDiasCancelacionCargo(Guid.Parse(reservation.LodgingId));

            SessionData.Reservation = reservation;

            SessionData.Reservation.PromotionPrice = SessionData.Reservation.TotalAmount;

            foreach (var vacancy in reservation.Vacancies)
            {

                using (var dc = new TurismoDataContext())
                {
                    var promociones = dc.Promociones_Alojamientos.Where(p => p.IDUNIDADPROMO.ToString() == vacancy.VacancyId && p.ACTIVO == true && p.FECHAINICIO <= vacancy.VacancyCheckin && p.FECHAFIN >= vacancy.VacancyCheckout);
                    vacancy.Promociones = promociones.ToArray();
                }
                if (vacancy.Promociones.Length > 0)
                {
                    SessionData.Reservation.TienePromocion = true;
                    foreach (var promocion in vacancy.Promociones)
                    {
                        if (promocion.IDTIPOPUBLICACIONPROMO == 1)
                        {
                            var nochesARestar = promocion.DIASRESERVADOS - promocion.DIASACOBRAR;
                            for (int i = 0; i < nochesARestar; i++)
                            {
                                SessionData.Reservation.PromotionPrice = SessionData.Reservation.PromotionPrice - vacancy.ConfirmedVacancyPrice;
                            }
                        }
                        else if (promocion.IDTIPOPUBLICACIONPROMO == 5)
                        {
                            SessionData.Reservation.PromotionPrice = SessionData.Reservation.PromotionPrice * (decimal.Parse(promocion.DESCUENTO.ToString()) / 100);
                        }
                        else if (promocion.IDTIPOPUBLICACIONPROMO == 6)
                        {
                            if (SessionData.Reservation.Vacancies.FirstOrDefault().VacancyReserved == 2)
                            {
                                var precioHabitacion1 = SessionData.Reservation.Vacancies.FirstOrDefault().ConfirmedVacancyPrice;
                                var precioHabtiacion2 = SessionData.Reservation.Vacancies.FirstOrDefault().ConfirmedVacancyPrice / 2;
                                SessionData.Reservation.PromotionPrice = precioHabitacion1 + precioHabtiacion2;
                            }
                        }
                    }
                }
            }

            ModelState.Clear();

            return Json(reservation, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BorrarHabitacion(string vacancyId)
        {
            int index = 0;
            int i = 0;
            foreach (var vacancy in SessionData.Reservation.Vacancies)
            {
                if (vacancy.VacancyId == vacancyId)
                {
                    index = i;
                }
                i++;
            }
            SessionData.Reservation.Vacancies.RemoveAt(index);
            return Json(SessionData.Reservation, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddPassenger(string travelerName, string travelerLastName, string travelerCountry, string travelerIdType, string travelerIdNumber, string vacancyId)
        {
            var traveler = new TravelerModel();
            if (string.IsNullOrEmpty(travelerIdNumber))
            {
                travelerIdNumber = "0";
            }

            if (SessionData.Reservation.ReservationOwner == null)
            {
                var reservationOwner = new TravelerModel()
                {
                    TravelerFirstName = travelerLastName,
                    TravelerLastName = travelerLastName,
                    TravelerCountry = travelerCountry,
                    TravelerIdType = IdType.DNI,
                    TravelerId = travelerIdNumber
                };
                SessionData.Reservation.ReservationOwner = reservationOwner;
            }

            foreach (var vacancy in SessionData.Reservation.Vacancies)
            {
                if (vacancy.VacancyId == vacancyId)
                {

                    traveler.TravelerFirstName = travelerName;
                    traveler.TravelerLastName = travelerLastName;
                    traveler.TravelerCountry = travelerCountry;
                    traveler.TravelerIdType = IdType.DNI;
                    traveler.TravelerId = travelerIdNumber;
                    vacancy.Rooms.Min().Travelers.Add(traveler);
                }
            }

            return Json(traveler, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EliminarPasajero(string nombre, string apellido)
        {
            string nombrePasajero = nombre + "-" + apellido;
            int index = 0;
            foreach (var vacancy in SessionData.Reservation.Vacancies)
            {
                int i = 0;
                bool found = false;
                foreach (var traveler in vacancy.Rooms.Min().Travelers)
                {
                    string travelerName = traveler.TravelerFirstName + "-" + traveler.TravelerLastName;
                    if (travelerName == nombrePasajero)
                    {
                        index = i;
                        found = true;
                    }
                    i++;
                }
                if (found)
                {
                    vacancy.Rooms.Min().Travelers.RemoveAt(index);
                } 
            }
            return Json(SessionData.Reservation, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OpenConfirmation()
        {
            return View("~/Views/Shared/Reservation.cshtml", SessionData.Reservation);
        }

        public ActionResult AddSecondaryUser(string SecondaryUserName, string SecondaryUserPass)
        {
            if (!string.IsNullOrEmpty(SecondaryUserName) || !string.IsNullOrEmpty(SecondaryUserPass)) 
            {
                SessionData.Reservation.SecondaryUserName = SecondaryUserName;
                SessionData.Reservation.SecondaryUserPass = SecondaryUserPass;
            }
            return null;
        }

        public ActionResult PaymentMethods()
        {
            //if (SessionData.Reservation == null || reservationModel == null)
            //    RedirectToAction("PaymentError");

            //SessionData.Reservation.LodgingId = reservationModel.LodgingId;
            //SessionData.Reservation.ReservationOwner = SessionData.Reservation.Vacancies.Min().Rooms.Min().Travelers.Min();
            //SessionData.Reservation.PaymentMethodId = reservationModel.PaymentMethodId;
            //SessionData.Reservation.Observations = reservationModel.Observations;
            //SessionData.Reservation.Vacancies = reservationModel.Vacancies;
            var manager = new Manager(SessionData.SearchType);
            SessionData.Reservation = manager.BlockVacancies(SessionData.Reservation, SessionData.UserCredential);
            SessionData.Reservation.DiasCancelacionCargo = manager.GetDiasCancelacionCargo(Guid.Parse(SessionData.Reservation.LodgingId));
            {

            }
            //return Redirect("http://200.85.184.11/SimuladorPagos/HSBC.aspx");
            //return RedirectToAction("InternalPaymentSimulator");

            ModelState.Clear();

            if (!SessionData.User.UsesPaymentInterface)
            {
                // Se utiliza la forma de pago por defecto del Web Service
                SessionData.Reservation.PaymentMethodId = Guid.Empty.ToString();
                return RedirectToAction("PaymentSuccess");
            }

            return View(SessionData.Reservation);
        }

        public ActionResult PaymentSuccess()
        {
            return View(SessionData.Reservation);
        }

        public ActionResult PaymentError()
        {
            return View();
        }

        public ActionResult InternalPaymentSimulator()
        {
            return View();
        }

        public ActionResult ProcessPaymentSimulation(string processAction, string clave)
        {
            if(processAction == "Accept" && clave == "ad123ar")
                return RedirectToAction("PaymentSuccess");
            else
                return RedirectToAction("PaymentError");
        }

		#region NPS

		public ActionResult ProcessPaymentNPS(ReservationModel reservation)
        {
			try
			{
				var npsBLL = new NpsBLL();

				var dto = npsBLL.GuardarPagoNPS(new PagoNPSDTO { ReservationId = long.Parse(SessionData.Reservation.ReservationId), IdEstadoNPS = (int)EstadoNPS.Iniciada, FechaGeneracion = DateTime.Now });

				using (var dc = new TurismoDataContext())
				{
					var formaPago = dc.FormaPagos.SingleOrDefault(fp => fp.IDFORMAPAGO == Guid.Parse(reservation.PaymentMethodId));
					var moneda = dc.MonedaDBs.SingleOrDefault(m => m.DESCRIPCION == MapCurrencyFromNPS(SessionData.Reservation.LodgingCurrencyCode));

					var response = npsBLL.ObtenerDatosPostTC(dto.IdPagoNPS.ToString(), SessionData.Reservation.ReservationId,
						(float)SessionData.Reservation.TotalAmount * moneda.COTIZACION, 1, "032", //currencyDb.CODIGO_NPS,
						formaPago.DESCRIPCION.Trim(), false, 0,
						SessionData.Reservation.ReservationOwner.TravelerId, SessionData.Reservation.ReservationOwner.TravelerEmail);

					dto.IdTransaccionNPS = response.TransactionId;
					dto.ResponseCod = response.ResponseCod;
					dto.ResponseMsg = response.ResponseMsg;
					dto.ResponseExtended = response.ResponseExtended;
					npsBLL.GuardarPagoNPS(dto);

					if (response.ResponseCod == (int)EstadoNPS.AprobadaAutorizada)
					{
						SessionData.Reservation.PaymentMethodId = reservation.PaymentMethodId;
						return View(new NPSRedirectionModel { FrontPSP_URL = response.FrontPSP_URL });
					}
					else
						return Redirect(Request.UrlReferrer.AbsoluteUri);
				}
			}
			catch (Exception)
			{
				return RedirectToAction("PaymentError");
			}
        }

        public ActionResult BridgeProcessResultPaymentNPS(NPSPaymentModel model)
        {
            return View(model);
        }

        public ActionResult ProcessResultPaymentNPS(NPSPaymentModel model)
        {
            var npsBLL = new NpsBLL();

            if(new ServiceController().CompleteReservation())
            {
				var response = npsBLL.ActualizarEstadoPago(model.psp_MerchTxRef);

                if (string.IsNullOrEmpty(response))
                    return RedirectToAction("PaymentSuccess");
                else
                    return RedirectToAction("PaymentError");
            }
            else
                return RedirectToAction("PaymentError");
        }

		
		//public JsonResult ObtenerDatosPostTC(long idPago)
		//{
		//    try
		//    {
		//        var result = pagoSrv.ObtenerDatosPostNPS(idPago);
		//        return Json(result);
		//    }
		//    catch (Exception)
		//    {

		//        throw;
		//    }
		//}

		//[HttpPost]
		//[AllowAnonymous]
		//public ActionResult CallbackNPS()
		//{
		//	ViewBag.HasErrors = false;
		//	ViewBag.Respuesta = string.Empty;

		//	string respuesta, merchTrxRef = string.Empty, idRequest;

		//	foreach (string name in Request.Form.AllKeys)
		//	{
		//		if (name == "psp_MerchTxRef")
		//		{
		//			merchTrxRef = Request.Form[name];
		//		}
		//	}

		//	long idPago;

		//	if (!long.TryParse(merchTrxRef, out idPago))
		//		respuesta = "Pago no válido. Contacte a su administrador.";
		//	else
		//	{
		//		idRequest = SessionHelper.IdRequest;
		//		respuesta = pagoSrv.ActualizarEstadoPagoNPS(idPago, idRequest);

		//	}

		//	ViewBag.Respuesta = idPago + " - " + respuesta;

		//	if (respuesta != string.Empty) ViewBag.HasErrors = true;

		//	return View();
		//}


		//[AllowAnonymous]
		//public ActionResult CallBackNPSVolver()
		//{
		//	return View();
		//}

		#endregion

		public ActionResult SendEmailReservation(EmailReservationModel model)
        {
            model.TravelerId = !string.IsNullOrEmpty(model.TravelerId) ? model.TravelerId : "0"; 

            using (var smtp = ObtenerClienteSmtp())
            {
                FluentEmail.Email
                .From(model.TravelerEmail)
                .Subject(string.Format("Reserva en {0}", model.LodgingName))
                .To("manzur.anibal@gmail.com")
                .Body("Datos de la reserva: <br>"+
                    "Nombre del Hotel: " + model.LodgingName + "<br>" +
                    "Ciudad: " + model.DestinationName + "<br>" +
                    "Nombre del Pasajero: " + model.TravelerName + "<br>" +
                    "Nacionalidad del Pasajero: " + model.Nationality + "<br>" +
                    "Dni del Pasajero: " + model.TravelerId + "<br>" +
                    "Cantidad de Pasajeros: " + model.TravelersCount + "<br>" +
                    "Checkin: " + model.CheckinDate + "<br>" +
                    "Checkout: " + model.CheckoutDate + "<br>" +
                    "Tipo de Habitacion: " + model.RoomType + "<br>" +
                    "Cantidad de Habitaciones: " + model.RoomsCount + "<br>" +
                    "Observaciones: " + model.Observations)
                .UsingClient(smtp)
                .Send();
            }
            return View("ReservationEmailSent", model);
#if true

#endif
        }

        public ActionResult ProcessPaymentCtaCte()
        {
            SessionData.Reservation.PaymentMethodId = "7d5192ca-fe10-455e-b051-d1023a07ba75";
            if (new ServiceController().CompleteReservation())
                return RedirectToAction("PaymentSuccess");
            else
                return RedirectToAction("PaymentError");

        }

        public ActionResult ProcessPaymentDeposito()
        {
            SessionData.Reservation.PaymentMethodId = "b8b3354a-4cd1-47dc-8267-707fd80d3072";
            if (new ServiceController().CompleteReservation())
                return RedirectToAction("PaymentSuccess");
            else
                return RedirectToAction("PaymentError");
        }

        private string MapCurrencyFromNPS(string code)
        {
            string currency = "ARS";

            switch (code)
            {
                case "032": currency = "ARS"; break;
                case "978": currency = "EUR"; break;
                case "840": currency = "USD"; break;
            }

            return currency;
        }

        private static SmtpClient ObtenerClienteSmtp()
        {
            var smtp = new SmtpClient();

            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = Config.LeerSetting("MailUseSSL", false);
            smtp.Host = Config.LeerSetting("MailServer");
            smtp.Port = Config.LeerSetting("MailPort", 25);

            string user = Config.LeerSetting("MailUser");

            if (!String.IsNullOrEmpty(user))
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(Config.LeerSetting("MailUser"),
                        Config.LeerSetting("MailPassword"));
            }
            return smtp;
        }
    }
}
