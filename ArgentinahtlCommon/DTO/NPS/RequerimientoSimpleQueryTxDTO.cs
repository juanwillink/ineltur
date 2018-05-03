using System;

namespace ArgentinahtlCommon.DTO.NPS
{
	public class RequerimientoSimpleQueryTxDTO
	{
		public string Version { get; set; }
		public string MerchantId { get; set; }
		public string QueryCriteria { get; set; }
		public string QueryCriteriaId { get; set; }
		public DateTime PosDateTime { get; set; }

		public string SecureHash { get; set; }
		public string SecretKey { get; set; }
	}
}
