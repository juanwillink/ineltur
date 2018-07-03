using System;

namespace ArgentinahtlCommon.DTO.NPS
{
	public class RespuestaCaptureDTO
	{
		public RespuestaCaptureDTO()
		{

		}
		public int ResponseCod { get; set; }
		public string ResponseMsg { get; set; }
		public string ResponseExtended { get; set; }
		public string MerchantId { get; set; }
		public string MerchTxRef { get; set; }

		public DateTime PosDateTime { get; set; }

		public long TransactionId { get; set; }
		public long TransactionIdOrigin { get; set; }

		//Propio de Argentinahtl
		public string ErrorMessage { get; set; }
	}
}