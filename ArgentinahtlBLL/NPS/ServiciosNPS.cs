using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using ArgentinahtlCommon;
using ArgentinahtlCommon.DTO.NPS;

namespace ArgentinahtlBLL.NPS
{
    public class ServiciosNPS
    {
        private string _UrlNPS;
        public ServiciosNPS(string Url)
        {
            _UrlNPS = Url;
        }
        public RespuestaPayOnline3pDTO PayOnLine_3p(RequerimientoPayOnline3pDTO dto)
        {
            //using (var service = new PaymentServicePlatform())
            using (var service = NPSWSServiceWrapper.GetService(_UrlNPS))
            {
                try
                {
                    
                    string secureHash = HashNPS.ObtenerHashPayOnline3p(dto);

                    var response = service.PayOnLine_3p(new RequerimientoStruct_PayOnLine_3p
                    {
                        psp_Version = dto.Version,
                        psp_MerchantId = dto.MerchantId,
                        psp_TxSource = dto.TxSource,
                        psp_MerchTxRef = dto.MerchTxRef,
                        psp_MerchOrderId = dto.MerchOrderId,
                        psp_ReturnURL = dto.ReturnURL,
                        psp_FrmLanguage = dto.FrmLanguage,
                        psp_FrmBackButtonURL = dto.FrmBackButtonURL,
                        psp_Amount = (dto.Amount == 0 ? null : Math.Truncate(dto.Amount * 100).ToString()),
                        psp_NumPayments = dto.NumPayments.ToString(),
                        //psp_PaymentAmount = (dto.PaymentAmount == 0 ? null : Math.Truncate(dto.PaymentAmount * 100).ToString()),
                        psp_Plan = (string.IsNullOrEmpty(dto.Plan) ? null : dto.Plan),
                        psp_Currency = dto.Currency,
                        psp_Country = dto.Country,
                        psp_Product = dto.Product,
                        //psp_CustomerId = dto.CustomerId,
                        psp_CustomerMail = dto.CustomerMail,
                        psp_MerchantMail = dto.MerchantMail,
                        psp_PurchaseDescription = dto.PurchaseDescription,
                        psp_PromotionCode = dto.PromotionCode,
                        psp_FirstPaymentDeferral = (dto.FirstPaymentDeferralDate.Year != 1 ? dto.FirstPaymentDeferralDate.ToString("yyyy-MM-dd") : null),
                        psp_PosDateTime = dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        psp_3dSecureAction = dto.TresDSecureAction,
                        psp_SecureHash = secureHash
                    }.AssignNullToEmptyStrings());
                    
                    return new DTOGenerador().ObtenerRespuestaPayOnline3pDTO(response);

                }
                catch (Exception ex)
                {
                    Tracker.WriteTrace(string.Format($"Error en metodo ServiciosNPS.PayOnLine_3p: {ex.Message}. Datos Enviados: {Tracker.SerializarObjeto(dto)}"), false, Tracker.TraceType.Error);
                    var response = new DTOGenerador().ObtenerRespuestaPayOnline3pDTO(null);
                    response.ErrorMessage = ex.Message + "\nINELPOL3P Inner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);

                    return response;
                }
            }
        }

        public RespuestaSimpleQueryTxDTO SimpleQueryTx(RequerimientoSimpleQueryTxDTO dto)
        {
            //using (var service = new PaymentServicePlatform())
            using (var service = NPSWSServiceWrapper.GetService(_UrlNPS))
            {
                try
                {
                    string secureHash = HashNPS.ObtenerHashSimpleQueryTx(dto);

                    var response = service.SimpleQueryTx(new RequerimientoStruct_SimpleQueryTx
                    {
                        psp_Version = dto.Version,
                        psp_MerchantId = dto.MerchantId,
                        psp_QueryCriteria = dto.QueryCriteria,
                        psp_QueryCriteriaId = dto.QueryCriteriaId,
                        psp_PosDateTime = dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        psp_SecureHash = secureHash
                    }.AssignNullToEmptyStrings());

                    return new DTOGenerador().ObtenerRespuestaSimpleQueryTxDTO(response);
                }
                catch (Exception ex)
                {
                    Tracker.WriteTrace(string.Format($"Error en metodo ServiciosNPS.SimpleQueryTx: {ex.Message}. Datos Enviados: {Tracker.SerializarObjeto(dto)}"), false, Tracker.TraceType.Error);
                    var response = new DTOGenerador().ObtenerRespuestaSimpleQueryTxDTO(null);
                    response.ErrorMessage = ex.Message + "\nInner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);

                    return response;
                }
            }
        }

		public RespuestaAuthorize3pDTO Authorize_3p(RequerimientoAuthorize3pDTO dto)
		{
			//using (var service = new PaymentServicePlatform())
			using (var service = NPSWSServiceWrapper.GetService(_UrlNPS))
			{
				try
				{
					string secureHash = HashNPS.ObtenerHashAuthorize3p(dto);

					var response = service.Authorize_3p(new RequerimientoStruct_Authorize_3p
					{
						psp_Version = dto.Version,
						psp_MerchantId = dto.MerchantId,
						psp_TxSource = dto.TxSource,
						psp_MerchTxRef = dto.MerchTxRef,
						psp_MerchOrderId = dto.MerchOrderId,
						psp_ReturnURL = dto.ReturnURL,
						psp_FrmLanguage = dto.FrmLanguage,
						psp_FrmBackButtonURL = dto.FrmBackButtonURL,
						psp_Amount = (dto.Amount == 0 ? null : Math.Truncate(dto.Amount * 100).ToString()),
						psp_NumPayments = dto.NumPayments.ToString(),
						//psp_PaymentAmount = (dto.PaymentAmount == 0 ? null : Math.Truncate(dto.PaymentAmount * 100).ToString()),
						psp_Plan = (string.IsNullOrEmpty(dto.Plan) ? null : dto.Plan),
						psp_Currency = dto.Currency,
						psp_Country = dto.Country,
						psp_Product = dto.Product,
						//psp_CustomerId = dto.CustomerId,
						psp_CustomerMail = dto.CustomerMail,
						psp_MerchantMail = dto.MerchantMail,
						psp_PurchaseDescription = dto.PurchaseDescription,
						psp_PromotionCode = dto.PromotionCode,
						//psp_FirstPaymentDeferral = (dto.FirstPaymentDeferralDate.Year != 1 ? dto.FirstPaymentDeferralDate.ToString("yyyy-MM-dd") : null),
						psp_PosDateTime = dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
						psp_3dSecureAction = dto.TresDSecureAction,
						psp_SecureHash = secureHash
					}.AssignNullToEmptyStrings());

					return new DTOGenerador().ObtenerRespuestaAuthorize3pDTO(response);

				}
				catch (Exception ex)
				{
					Tracker.WriteTrace(string.Format($"Error en metodo ServiciosNPS.Authorize_3p: {ex.Message}. Datos Enviados: {Tracker.SerializarObjeto(dto)}"), false, Tracker.TraceType.Error);
					var response = new DTOGenerador().ObtenerRespuestaAuthorize3pDTO(null);
					response.ErrorMessage = ex.Message + "\nINELPOL3P Inner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);

					return response;
				}
			}
		}

		public RespuestaCaptureDTO Capture3p(RequerimientoCapture3pDTO dto)
		{
			//using (var service = new PaymentServicePlatform())
			using (var service = NPSWSServiceWrapper.GetService(_UrlNPS))
			{
				try
				{
					string secureHash = HashNPS.ObtenerHashCapture(dto);

					var response = service.Capture(new RequerimientoStruct_Capture
					{
						psp_Version = dto.Version,
						psp_MerchantId = dto.MerchantId,
						psp_TxSource = dto.TxSource,
						psp_TransactionId_Orig = dto.TransactionId_Orig,
						psp_MerchTxRef = dto.MerchTxRef,
						psp_AmountToCapture = dto.AmountToCapture,
						psp_PosDateTime = dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
						psp_SecureHash = secureHash
					}.AssignNullToEmptyStrings());

					return new DTOGenerador().ObtenerRespuestaCaptureDTO(response);
				}
				catch (Exception ex)
				{
					Tracker.WriteTrace(string.Format($"Error en metodo ServiciosNPS.Capture3p: {ex.Message}. Datos Enviados: {Tracker.SerializarObjeto(dto)}"), false, Tracker.TraceType.Error);
					var response = new DTOGenerador().ObtenerRespuestaCaptureDTO(null);
					response.ErrorMessage = ex.Message + "\nInner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);

					return response;
				}
			}
		}
	}
}
