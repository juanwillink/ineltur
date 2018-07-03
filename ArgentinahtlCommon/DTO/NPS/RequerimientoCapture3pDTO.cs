using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgentinahtlCommon.DTO.NPS
{
	public class RequerimientoCapture3pDTO
	{
		public string Version { get; set; }
		public string MerchantId { get; set; }
		public string TxSource { get; set; }
		public string MerchTxRef { get; set; }
		public string psp_MerchAdditionalRefField { get; set; }
		public string TransactionId_Orig { get; set; }
		public string AmountToCapture { get; set; }
		public string UserId { get; set; }

		public DateTime PosDateTime { get; set; }

		public string SecureHash { get; set; }
		public string SecretKey { get; set; }

	}
}

