using System;

namespace ArgentinahtlCommon.DTO.NPS
{
	public class RespuestaSimpleQueryTxDTO
	{
		public RespuestaSimpleQueryTxDTO()
		{
			Transaction = new TransactionNpsDTO();
		}
		public int ResponseCod { get; set; }
		public string ResponseMsg { get; set; }
		public string ResponseExtended { get; set; }
		public string MerchantId { get; set; }
		public string QueryCriteria { get; set; }
		public string QueryCriteriaId { get; set; }
		public DateTime PosDateTime { get; set; }

		public TransactionNpsDTO Transaction { get; set; }

		//Propio de Argentinahtl
		public string ErrorMessage { get; set; }
	}
}
