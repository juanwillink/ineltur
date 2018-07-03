using System;

namespace ArgentinahtlCommon.DTO.NPS
{
	public class RequerimientoAuthorize3pDTO
	{
		public string Version { get; set; }
		public string TxSource { get; set; }
		public string FrmLanguage { get; set; }
		public string Country { get; set; }
		public string ReturnURL { get; set; }
		public string FrmBackButtonURL { get; set; }

		public string MerchantId { get; set; }
		public string SecretKey { get; set; }
		//datos de la transacción------
		public string MerchOrderId { get; set; }
		public string MerchTxRef { get; set; }
		public float Amount { get; set; }
		public int NumPayments { get; set; }

		public string Currency { get; set; }
		public string Product { get; set; }
		public string CustomerId { get; set; }
		public string CustomerMail { get; set; }

		public DateTime PosDateTime { get; set; }
		public string MerchantMail { get; set; }
		public string TresDSecureAction { get; set; }

		public string Plan { get; set; }
		public string PurchaseDescription { get; set; }
		public string PromotionCode { get; set; }

	}
}
