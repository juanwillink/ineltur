using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.Security.Cryptography;
using ArgentinahtlCommon;
using ArgentinahtlCommon.DTO.NPS;


namespace ArgentinahtlBLL.NPS
{
    internal static class NPSWSServiceWrapper
    {
        internal static PaymentServicePlatformPortTypeClient GetService(string siteUrl)
        {
            var basicHttpSecurityMode = BasicHttpSecurityMode.None;

            if (siteUrl.StartsWith("https"))
                basicHttpSecurityMode = BasicHttpSecurityMode.Transport;

			System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

			Uri serviceUri = new Uri(siteUrl);

			AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("SOAPAction", "https://implementacion.nps.com.ar/ws.php/Authorize_3p", 1);
			AddressHeader[] addressHeaders1 = new AddressHeader[1] {addressHeader1};

			EndpointAddress endpointAddress = new EndpointAddress(serviceUri, addressHeaders1);

            //Create the binding here
            Binding binding = new CustomBinding(BindingFactory.CreateInstance(basicHttpSecurityMode));

            PaymentServicePlatformPortTypeClient client = new PaymentServicePlatformPortTypeClient(binding, endpointAddress);

            client.Endpoint.Behaviors.Add(new FaultFormatingBehavior());
			
            return client;
        }

        
    }

    internal static class BindingFactory
    {
        internal static Binding CreateInstance(BasicHttpSecurityMode basicHttpSecurityMode)
        {
            //ConfiguracionBLL conf = new ConfiguracionBLL();
            //string proxy = conf.ObtenerConfiguracion((int)ConfiguracionEnum.NPSBindingProxy).Valor;
            //string useDefaultProxy = conf.ObtenerConfiguracion((int)ConfiguracionEnum.NPSBindingUseDefaultWebProxy).Valor;

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = basicHttpSecurityMode;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            //binding.UseDefaultWebProxy =  !string.IsNullOrEmpty(useDefaultProxy) && useDefaultProxy.ToUpper() != "NO" ? true : false;
            //binding.ProxyAddress = new Uri(proxy);

            binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            binding.ReaderQuotas.MaxDepth = int.MaxValue;
            binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
			
            //binding.BypassProxyOnLocal
            return binding;
        }

    }

    internal class FaultFormatingBehavior : IEndpointBehavior
    {
        //---------
        //private string _serviceURL; 
        //public FaultFormatingBehavior(string serviceURL) { _serviceURL = serviceURL; }

        //----------
        public void Validate(ServiceEndpoint endpoint) { }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher){ }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new FaultMessageInspector());
        }

    }

    public class FaultMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            try
            {
				Tracker.WriteTrace($"FaultMessageInspector.BeforeSendRequest - Request: {request} ");
            }
            catch (Exception e)
            {
                Tracker.WriteTrace($"FaultMessageInspector.BeforeSendRequest - Exception: {e.Message} ") ;
            }
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            try
            {
				Tracker.WriteTrace($"FaultMessageInspector.AfterReceiveReply - Response: {reply} ");

                if (reply.IsFault)
                {
                    using (XmlDictionaryReader reader = reply.GetReaderAtBodyContents())
                    {
                        XmlDocument document = new XmlDocument();
                        document.Load(reader);
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                        //string xml = reader.ReadInnerXml();
                        XmlNode detailNode = document.SelectSingleNode("//detail", nsmgr);
                        XmlNode faultCodeNode = document.SelectSingleNode("//faultcode", nsmgr);

                        XmlNode faultStringNode = document.SelectSingleNode("//faultstring", nsmgr);

                        FaultCode fc = new FaultCode(faultCodeNode.FirstChild.InnerText);

                        Message newFaultMessage = Message.CreateMessage(reply.Version, fc, faultStringNode.FirstChild.InnerText, "");
                        reply = newFaultMessage;

                    }

                }
               

            }
            catch (Exception e)
            {
				Tracker.WriteTrace($"FaultMessageInspector.AfterReceiveReply - Exception: {e.Message} ");
            }
        }

    }



    internal static class HashNPS 
    {
        internal static string ObtenerHashPayOnline3p(RequerimientoPayOnline3pDTO dto) 
        {
            string hash = string.Empty;

            try
            {
                hash =
                        dto.TresDSecureAction +
                        (dto.Amount == 0 ? null : Math.Truncate(dto.Amount * 100).ToString()) +
                        dto.Country +
                        dto.Currency +
                        //dto.CustomerId +
                        dto.CustomerMail +
                        (dto.FirstPaymentDeferralDate.Year != 1 ? dto.FirstPaymentDeferralDate.ToString("yyyy-MM-dd") : null) +
                        dto.FrmBackButtonURL +
                        dto.FrmLanguage +
                        dto.MerchOrderId +
                        dto.MerchTxRef +
                        dto.MerchantId +
                        dto.MerchantMail +
                        dto.NumPayments.ToString() +
                        //(dto.PaymentAmount == 0 ? null : Math.Truncate(dto.PaymentAmount * 100).ToString()) +
                        (string.IsNullOrEmpty(dto.Plan) ? null : dto.Plan) +
                        dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss") +
                        dto.Product +
                        dto.PromotionCode +
                        dto.PurchaseDescription +
                        dto.ReturnURL +
                        dto.TxSource +
                        dto.Version +
                        dto.SecretKey;
            }
            catch(Exception)
            {
                throw;
            }

            return CodificarHash(hash);

        }

 

        internal static string ObtenerHashSimpleQueryTx(RequerimientoSimpleQueryTxDTO dto)
        {
            string hash = string.Empty;

            try
            {
                hash =  dto.MerchantId +
                        dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss") +
                        dto.QueryCriteria +
                        dto.QueryCriteriaId +
                        dto.Version +
                        dto.SecretKey;

            }
            catch (Exception)
            {
                throw;
            }

            return CodificarHash(hash);

        }

		internal static string ObtenerHashAuthorize3p(RequerimientoAuthorize3pDTO dto)
		{
			string hash = string.Empty;

			try
			{
				hash =
						dto.TresDSecureAction +
						(dto.Amount == 0 ? null : Math.Truncate(dto.Amount * 100).ToString()) +
						dto.Country +
						dto.Currency +
						//dto.CustomerId +
						dto.CustomerMail +
						//(dto.FirstPaymentDeferralDate.Year != 1 ? dto.FirstPaymentDeferralDate.ToString("yyyy-MM-dd") : null) +
						dto.FrmBackButtonURL +
						dto.FrmLanguage +
						dto.MerchOrderId +
						dto.MerchTxRef +
						dto.MerchantId +
						dto.MerchantMail +
						dto.NumPayments.ToString() +
						//(dto.PaymentAmount == 0 ? null : Math.Truncate(dto.PaymentAmount * 100).ToString()) +
						(string.IsNullOrEmpty(dto.Plan) ? null : dto.Plan) +
						dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss") +
						dto.Product +
						dto.PromotionCode +
						dto.PurchaseDescription +
						dto.ReturnURL +
						dto.TxSource +
						dto.Version +
						dto.SecretKey;
			}
			catch (Exception)
			{
				throw;
			}

			return CodificarHash(hash);

		}

		internal static string ObtenerHashCapture(RequerimientoCapture3pDTO dto)
		{
			string hash = string.Empty;

			try
			{
				hash = dto.AmountToCapture +
						dto.MerchTxRef +
						dto.MerchantId +
						dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss") +
						dto.TransactionId_Orig +
						dto.TxSource +
						dto.Version +
						dto.SecretKey;

			}
			catch (Exception)
			{
				throw;
			}

			return CodificarHash(hash);

		}

		

		

		private static string CodificarHash(string hash)
        {
            string computedHash = string.Empty;

            try
            {
                computedHash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(hash))
                .Select(b => b.ToString("x2"))
                .Aggregate((i, j) => i + j);
            }
            catch(Exception)
            {
                throw;
            }

            return computedHash;
        }
    }

    internal class DTOGenerador
    {
        internal RespuestaPayOnline3pDTO ObtenerRespuestaPayOnline3pDTO(RespuestaStruct_PayOnLine_3p originalResponse)
        {
            RespuestaPayOnline3pDTO dto = new RespuestaPayOnline3pDTO();

            if (originalResponse != null)
            {
                dto.ResponseCod = Convert.ToInt32(originalResponse.psp_ResponseCod);
                dto.ResponseMsg = originalResponse.psp_ResponseMsg;
                dto.ResponseExtended = originalResponse.psp_ResponseExtended;
                dto.TransactionId = Convert.ToInt64(originalResponse.psp_TransactionId);
                dto.Session3p = originalResponse.psp_Session3p;
                dto.FrontPSP_URL = originalResponse.psp_FrontPSP_URL;
                dto.MerchantId = originalResponse.psp_MerchantId;
                dto.MerchTxRef = originalResponse.psp_MerchTxRef;
                dto.MerchOrderId = originalResponse.psp_MerchOrderId;
                dto.CustomerMail = originalResponse.psp_CustomerMail;
                dto.MerchantMail = originalResponse.psp_MerchantMail;
                dto.Plan = originalResponse.psp_Plan;
                dto.FirstPaymentDeferralDate = originalResponse.psp_FirstPaymentDeferral;
                dto.PosDateTime = Convert.ToDateTime(originalResponse.psp_PosDateTime);

				if (dto.ResponseCod != Convert.ToInt16(RespuestaSolicitudAutorizacionNPS.Exitosa))
                {
                    dto.ErrorMessage = dto.ResponseMsg;
                    if (dto.ResponseExtended != null)
                        dto.ErrorMessage += " Detalle: " + dto.ResponseExtended;
                }
            }

            return dto;
        }

		internal RespuestaAuthorize3pDTO ObtenerRespuestaAuthorize3pDTO(RespuestaStruct_Authorize_3p originalResponse)
		{
			RespuestaAuthorize3pDTO dto = new RespuestaAuthorize3pDTO();

			if (originalResponse != null)
			{
				dto.ResponseCod = Convert.ToInt32(originalResponse.psp_ResponseCod);
				dto.ResponseMsg = originalResponse.psp_ResponseMsg;
				dto.ResponseExtended = originalResponse.psp_ResponseExtended;
				dto.TransactionId = Convert.ToInt64(originalResponse.psp_TransactionId);
				dto.Session3p = originalResponse.psp_Session3p;
				dto.FrontPSP_URL = originalResponse.psp_FrontPSP_URL;
				dto.MerchantId = originalResponse.psp_MerchantId;
				dto.MerchTxRef = originalResponse.psp_MerchTxRef;
				dto.MerchOrderId = originalResponse.psp_MerchOrderId;
				dto.CustomerMail = originalResponse.psp_CustomerMail;
				dto.MerchantMail = originalResponse.psp_MerchantMail;
				dto.Plan = originalResponse.psp_Plan;
				//dto.FirstPaymentDeferralDate = originalResponse.psp_FirstPaymentDeferral;
				dto.PosDateTime = Convert.ToDateTime(originalResponse.psp_PosDateTime);

				if (dto.ResponseCod != Convert.ToInt16(RespuestaSolicitudAutorizacionNPS.Exitosa))
				{
					dto.ErrorMessage = dto.ResponseMsg;
					if (dto.ResponseExtended != null)
						dto.ErrorMessage += " Detalle: " + dto.ResponseExtended;
				}
			}

			return dto;
		}

		internal RespuestaSimpleQueryTxDTO ObtenerRespuestaSimpleQueryTxDTO(RespuestaStruct_SimpleQueryTx originalResponse)
        {
            RespuestaSimpleQueryTxDTO dto = new RespuestaSimpleQueryTxDTO();

            if (originalResponse != null)
            {
                dto.ResponseCod = Convert.ToInt32(originalResponse.psp_ResponseCod);
                dto.ResponseMsg = originalResponse.psp_ResponseMsg;
                dto.ResponseExtended = originalResponse.psp_ResponseExtended;
                dto.MerchantId = originalResponse.psp_MerchantId;
                dto.QueryCriteria = originalResponse.psp_QueryCriteria;
                dto.QueryCriteriaId = originalResponse.psp_QueryCriteriaId;
                dto.PosDateTime = Convert.ToDateTime(originalResponse.psp_PosDateTime);

                if (originalResponse.psp_ResponseCod == RespuestaSimpleQueryTxNPS.Exitosa)
                {
                    dto.Transaction.ResponseCod = Convert.ToInt32(originalResponse.psp_Transaction.psp_ResponseCod);
                    dto.Transaction.ResponseMsg = originalResponse.psp_Transaction.psp_ResponseMsg;
                    dto.Transaction.ResponseExtended = originalResponse.psp_Transaction.psp_ResponseExtended;
                    dto.Transaction.TransactionId = Convert.ToInt64(originalResponse.psp_Transaction.psp_TransactionId);
                }
            }

            return dto;
        }

		internal RespuestaCaptureDTO ObtenerRespuestaCaptureDTO(RespuestaStruct_Capture originalResponse)
		{
			RespuestaCaptureDTO dto = new RespuestaCaptureDTO();

			if (originalResponse != null)
			{
				dto.ResponseCod = Convert.ToInt32(originalResponse.psp_ResponseCod);
				dto.ResponseMsg = originalResponse.psp_ResponseMsg;
				dto.ResponseExtended = originalResponse.psp_ResponseExtended;
				dto.MerchantId = originalResponse.psp_MerchantId;
				dto.PosDateTime = Convert.ToDateTime(originalResponse.psp_PosDateTime);
				dto.TransactionId = Convert.ToInt64(originalResponse.psp_TransactionId);
				dto.TransactionIdOrigin = Convert.ToInt64(originalResponse.psp_TransactionId_Orig);
				dto.MerchTxRef = originalResponse.psp_MerchTxRef;

			}

			return dto;
		}
	}

    public static class Extensions
    {
        public static bool PerteneceALista<T>(T elemento, params T[] listaElementos)
        {
            foreach (T t in listaElementos)
            {
                if (elemento.Equals(t))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Evalúa la existencia del elemento dentro de una lista de elementos determinada.
        /// </summary>
        /// <param name="listaElementos">Lista de elementos del mismo tipo que el elemento a evaluar</param>
        /// <returns>true si el elemento se encuentra en la lista, false en caso contrario</returns>
        public static bool PerteneceA<T>(this T elemento, params T[] listaElementos)
        {
            return PerteneceALista(elemento, listaElementos);
        }
        public static object AssignNullToEmptyStrings(this object element)
        {
            try
            {
                if (element.GetType() == typeof(string) && string.IsNullOrEmpty((string)element))
                    element = null;
                else
                {
                    foreach (var p in element.GetType().GetProperties())
                    {
                        if (p.PropertyType == typeof(string) && string.IsNullOrEmpty((string)p.GetValue(element, null)))
                            p.SetValue(element, null, null);
                        else
                        {
                            if (p.GetIndexParameters().Length == 0 && !typeof(IEnumerable).PerteneceA(p.PropertyType.GetInterfaces()))
                                if (p.GetValue(element, null) != null)
                                    p.GetValue(element, null).AssignNullToEmptyStrings();
                                else
                                {
                                    if (p.GetValue(element, null) != null)
                                    {
                                        foreach (var o in p.GetValue(element, null) as IEnumerable)
                                            o.AssignNullToEmptyStrings();
                                    }
                                }
                        }
                    }
                }
            }
            catch (Exception)
            { }

            return element;
        }

        public static T AssignNullToEmptyStrings<T>(this T element)
        {
            return (T)((object)element).AssignNullToEmptyStrings();
        }
    }
    
}
