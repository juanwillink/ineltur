using System;

namespace ArgentinahtlCommon.DTO.NPS
{
	public class RespuestaPayOnline3pDTO
	{
		public int ResponseCod { get; set; }
		public string ResponseMsg { get; set; }
		public string ResponseExtended { get; set; }
		public long TransactionId { get; set; }
		public string Session3p { get; set; }
		public string FrontPSP_URL { get; set; }
		public string MerchantId { get; set; }
		public string MerchTxRef { get; set; }
		public string MerchOrderId { get; set; }
		public string CustomerMail { get; set; }
		public string CustomerId { get; set; }
		public string MerchantMail { get; set; }
		public string Plan { get; set; }
		public string FirstPaymentDeferralDate { get; set; }
		public DateTime PosDateTime { get; set; }

		//Propio de Ineltur
		public string ErrorMessage { get; set; }
	}
}
