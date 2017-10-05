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
using Reca.Common.Utils;
using Reca.Common.DTO.NPS;
using Reca.Common.Consts;


namespace Reca.BLL.NPS
{
    internal static class NPSWSServiceWrapper
    {
        internal static NPS.PaymentServicePlatformPortTypeClient GetService(string siteUrl)
        {
            var basicHttpSecurityMode = BasicHttpSecurityMode.None;

            if (siteUrl.StartsWith("https"))
                basicHttpSecurityMode = BasicHttpSecurityMode.Transport;

            Uri serviceUri = new Uri(siteUrl);

            EndpointAddress endpointAddress = new EndpointAddress(serviceUri);

            //Create the binding here
            Binding binding = new CustomBinding(BindingFactory.CreateInstance(basicHttpSecurityMode));

            PaymentServicePlatformPortTypeClient client = new PaymentServicePlatformPortTypeClient(binding, endpointAddress);

            client.Endpoint.Behaviors.Add(new FaultFormatingBehavior());

            return client;
        }

        internal static NPSTest.PaymentServicePlatformPortTypeClient GetServiceTest(string siteUrl)
        {
            var basicHttpSecurityMode = BasicHttpSecurityMode.None;

            if (siteUrl.StartsWith("https"))
                basicHttpSecurityMode = BasicHttpSecurityMode.Transport;

            Uri serviceUri = new Uri(siteUrl);

            EndpointAddress endpointAddress = new EndpointAddress(serviceUri);

            //Create the binding here
            Binding binding = new CustomBinding(BindingFactory.CreateInstance(basicHttpSecurityMode));

            NPSTest.PaymentServicePlatformPortTypeClient client = new NPSTest.PaymentServicePlatformPortTypeClient(binding, endpointAddress);

            client.Endpoint.Behaviors.Add(new FaultFormatingBehavior());

            return client;
        }
    }

    internal static class BindingFactory
    {
        internal static Binding CreateInstance(BasicHttpSecurityMode basicHttpSecurityMode)
        {
            ConfiguracionBLL conf = new ConfiguracionBLL();
            string proxy = conf.ObtenerConfiguracion((int)ConfiguracionEnum.NPSBindingProxy).Valor;
            string useDefaultProxy = conf.ObtenerConfiguracion((int)ConfiguracionEnum.NPSBindingUseDefaultWebProxy).Valor;

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = basicHttpSecurityMode;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            binding.UseDefaultWebProxy =  !string.IsNullOrEmpty(useDefaultProxy) && useDefaultProxy.ToUpper() != "NO" ? true : false;
            binding.ProxyAddress = new Uri(proxy);

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
                Globals.Logger.Info($"FaultMessageInspector.BeforeSendRequest - Request: {request} ");
            }
            catch (Exception e)
            {
                Globals.Logger.Error($"FaultMessageInspector.BeforeSendRequest - Exception: {ExceptionHelper.GetFullMessage(e)} ");
            }
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            try
            {
                Globals.Logger.Info($"FaultMessageInspector.AfterReceiveReply - Response: {reply} ");

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
                //else
                //{
                //    Message revised = null;
                //    var contents = new StringBuilder();
                //    var writer = XmlWriter.Create(contents);

                //    reply.WriteMessage(writer);
                //    writer.Flush();

                //    revised = Message.CreateMessage(reply.Version, reply.Headers.Action, XmlReader.Create(new StringReader(contents.ToString())));
                //    revised.Headers.CopyHeadersFrom(reply);
                //    revised.Properties.CopyProperties(reply.Properties);

                //    reply = revised;

                //    //XmlDocument doc = new XmlDocument();
                //    //MemoryStream ms = new MemoryStream();
                //    //XmlWriter writer = XmlWriter.Create(ms);
                //    //reply.WriteMessage(writer);
                //    ////writer.Flush();
                //    //ms.Position = 0;
                //    //doc.Load(ms);

                //    //ChangeMessage(ref doc);

                //    //ms.SetLength(0);
                //    //writer = XmlWriter.Create(ms);
                //    //doc.WriteTo(writer);
                //    ////writer.Flush();
                //    //ms.Position = 0;
                //    //XmlReader reader = XmlReader.Create(ms);
                //    //reply = Message.CreateMessage(reader, int.MaxValue, reply.Version);

                //}

            }
            catch (Exception e)
            {
                Globals.Logger.Error($"FaultMessageInspector.AfterReceiveReply - Exception: {ExceptionHelper.GetFullMessage(e)} ");
            }
        }

        //void ChangeMessage(ref XmlDocument doc)
        //{
            
        //    XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);

        //    nsManager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");

        //    nsManager.AddNamespace("tempuri", "http://tempuri.org/");

        //    XmlNode node = doc.SelectSingleNode("//ns1", nsManager);

        //    if (node != null)
        //    {
        //        XmlText text = node.FirstChild as XmlText;

        //        if (text != null)
        //        {
        //            text.Value = "Modified: " + text.Value;
        //        }
        //    }
        //}
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

        internal static string ObtenerHashCashPayment3p(RequerimientoCashPayment3pDTO dto)
        {
            string hash = string.Empty;

            try
            {
                hash =
                        (dto.Amount == 0 ? null : Math.Truncate(dto.Amount * 100).ToString()) +
                        dto.Country +
                        dto.Currency +
                        //(dto.CustomerDocNum == 0 ? null : dto.CustomerDocNum.ToString()) +
                        //dto.CustomerId +
                        dto.CustomerMail +
                        (dto.DaysAvailableToPay == 0 ? null : dto.DaysAvailableToPay.ToString()) +
                        dto.DaysUntilSecondExpDate.ToString() +
                        dto.FirstExpDate.ToString("yyyy-MM-dd") +
                        //dto.ForceProcessingMethod +
                        dto.FrmBackButtonURL +
                        dto.FrmLanguage +
                        //dto.MerchantMail +
                        dto.MerchOrderId +
                        dto.MerchTxRef +
                        dto.MerchantId +
                        dto.PosDateTime.ToString("yyyy-MM-dd HH:mm:ss") +
                        dto.Product.ToString() +
                        //dto.PurchaseDescription +
                        dto.ReturnURL +
                        dto.SurchargeAmount.ToString() +
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

                if (dto.ResponseCod != Convert.ToInt16(CodigoRespuestaSolicitudAutorizacionNPS.Exitosa))
                {
                    dto.ErrorMessage = dto.ResponseMsg;
                    if (dto.ResponseExtended != null)
                        dto.ErrorMessage += " Detalle: " + dto.ResponseExtended;
                }
            }

            return dto;
        }

        internal RespuestaCashPayment3pDTO ObtenerRespuestaCashPayment3pDTO(RespuestaStruct_CashPayment_3p originalResponse)
        {
            RespuestaCashPayment3pDTO dto = new RespuestaCashPayment3pDTO();

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
                dto.BarCode = originalResponse.psp_BarCode;
                dto.PosDateTime = Convert.ToDateTime(originalResponse.psp_PosDateTime);

                if (dto.ResponseCod != Convert.ToInt16(CodigoRespuestaSolicitudAutorizacionNPS.Exitosa))
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

                if (originalResponse.psp_ResponseCod == CodigoRespuestaSimpleQueryTxNPS.Exitosa)
                {
                    dto.Transaction.ResponseCod = Convert.ToInt32(originalResponse.psp_Transaction.psp_ResponseCod);
                    dto.Transaction.ResponseMsg = originalResponse.psp_Transaction.psp_ResponseMsg;
                    dto.Transaction.ResponseExtended = originalResponse.psp_Transaction.psp_ResponseExtended;
                    dto.Transaction.TransactionId = Convert.ToInt64(originalResponse.psp_Transaction.psp_TransactionId);
                }
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
