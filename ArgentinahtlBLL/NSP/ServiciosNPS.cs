using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Reca.Common.DTO.NPS;
using Reca.Common.Utils;


namespace Reca.BLL.NPS
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
                        psp_Product = dto.Product.ToString(),
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
                    Globals.Logger.Error(string.Format("Error en metodo ServiciosNPS.PayOnLine_3p: {0}. Datos Enviados: {1}", ex.Message, Utiles.SerializarObjeto(dto)), ex);
                    var response = new DTOGenerador().ObtenerRespuestaPayOnline3pDTO(null);
                    response.ErrorMessage = ex.Message + "\nIFREPOL3P Inner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);

                    return response;
                }
            }
        }

        public RespuestaCashPayment3pDTO CashPayment_3p(RequerimientoCashPayment3pDTO dto)
        {
            //using (var service = new PaymentServicePlatform())
            using (var service = NPSWSServiceWrapper.GetService(_UrlNPS))
            {
                try
                {
                    string secureHash = HashNPS.ObtenerHashCashPayment3p(dto);

                    var response = service.CashPayment_3p(new RequerimientoStruct_CashPayment_3p
                    {
                        psp_Amount = (dto.Amount == 0 ? null : Math.Truncate(dto.Amount * 100).ToString()),
                        //psp_BillingDetails = new BillingDetailsStruct() { Address = new AddressStruct() { Street = " ", HouseNumber = " ", City = " ", Country = dto.Country } },
                        psp_Country = dto.Country,
                        psp_Currency = dto.Currency,
                        //psp_CustomerDocNum = dto.CustomerDocNum == 0 ? null : dto.CustomerDocNum.ToString(),
                        //psp_CustomerId = dto.CustomerId,
                        psp_CustomerMail = dto.CustomerMail,
                        psp_DaysAvailableToPay = dto.DaysAvailableToPay == 0 ? null : dto.DaysAvailableToPay.ToString(),
                        psp_DaysUntilSecondExpDate = dto.DaysUntilSecondExpDate.ToString(),
                        psp_FirstExpDate = dto.FirstExpDate.ToString("yyyy-MM-dd"),
                        //psp_ForceProcessingMethod = dto.ForceProcessingMethod,
                        psp_FrmBackButtonURL = dto.FrmBackButtonURL,
                        psp_FrmLanguage = dto.FrmLanguage,
                        //psp_MerchantMail = dto.MerchantMail,
                        psp_MerchOrderId = dto.MerchOrderId,
                        psp_MerchTxRef = dto.MerchTxRef,
                        psp_MerchantId = dto.MerchantId,
                        psp_PosDateTime = dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        psp_Product = dto.Product.ToString(),
                        //psp_PurchaseDescription = dto.PurchaseDescription,
                        psp_ReturnURL = dto.ReturnURL,
                        psp_SurchargeAmount = dto.SurchargeAmount.ToString(),
                        psp_TxSource = dto.TxSource,
                        psp_Version = dto.Version,
                        psp_SecureHash = secureHash
                        

                    }.AssignNullToEmptyStrings());
                    return new DTOGenerador().ObtenerRespuestaCashPayment3pDTO(response);

                }
                catch (Exception ex)
                {
                    Globals.Logger.Error(string.Format("Error en metodo ServiciosNPS.CashPayment_3p: {0}. Datos Enviados: {1}", ex.Message, Utiles.SerializarObjeto(dto)), ex);
                    var response = new DTOGenerador().ObtenerRespuestaCashPayment3pDTO(null);
                    response.ErrorMessage = ex.Message + "\nIFRECP3P Inner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);

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
                    Globals.Logger.Error(string.Format("Error en metodo ServiciosNPS.SimpleQueryTx: {0}. Datos Enviados: {1}", ex.Message, Utiles.SerializarObjeto(dto)), ex);
                    var response = new DTOGenerador().ObtenerRespuestaSimpleQueryTxDTO(null);
                    response.ErrorMessage = ex.Message + "\nInner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);

                    return response;
                }
            }
        }
    }
}
