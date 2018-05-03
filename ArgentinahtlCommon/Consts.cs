namespace ArgentinahtlCommon
{
    public class Consts
    {
		public static string urlRaizSitio= "http://localhost:52233/";

	}

	#region NPS
	public class NPSConfiguracion
    {
        public static string Url = "https://implementacion.nps.com.ar/ws.php";
        public static string Version = "2.2";
        public static string TxSource = "WEB";
        public static string FrmLanguage = "es_AR";
        public static string Country = "ARG";
        public static string MerchantID = "ineltur";
		public static string NPSSecretKey = "bPlA1UnJiBp3usAXE1jc6ZsaAk6ewk13bhDzFrgzf9KKDLy0b5Sw6jk6hTAdiabR";
		public static string ReturnURL = "Payment/ProcessResultPaymentNPS";
		public static string FrmBackButtonURL = "Payment/ProcessResultPaymentNPS"; 
		public static string MerchantMail = "manzur.anibal@gmail.com";
		public static string QueryCriteriaMerchTxRef = "M";
	}

	public class RespuestaSimpleQueryTxNPS
	{
		public const string Exitosa = "2";

	}

	public class RespuestaSolicitudAutorizacionNPS
	{
		public const string Exitosa = "1";

	}

	public enum EstadoNPS
	{
		AprobadaAutorizada = 1,
		DeclinadaNoAutorizadaFondosInsuficientes = 2,
		DeclinadaNoAutorizadaTarjetaVencida = 3,
		DeclinadaNoAutorizadaCodigoDeSeguridadInvalido = 4,
		DeclinadaNoAutorizadaTarjetaInvalida = 5,
		DeclinadaNoAutorizada = 6,
		DeclinadaTarjetaYExpiracionHanCambiado = 7,
		RechazadaNoAutenticada = 8,
		RechazadaSolicitudDelComercioInvalida = 9,
		RechazadaErrorDeProcesamientoEnPSP = 10,
		RechazadaTimedOut = 11,
		RechazadaErrorDeProcesamientoEnSP = 12,
		RechazadaReversada = 13,
		RechazadaExpirada = 14,
		RechazadaPorSospechaDeFraude = 15,
		EnCursoEnComercio = 16,
		EnCursoEnComprador_NPSWebsite = 17,
		EnCursoTransaccionEnProgreso = 18,
		PendienteEnComprador_CashPayment = 19,
		PendienteEnComprador_BankPayment = 20,
		PendienteEnAutorizador = 21,
		Iniciada = 22

	}

	public class NPSVerifiedByVisa
	{
		public const string AutorizarSinAutenticacion3dSecure = "0";
		public const string AutorizarSiAutenticacion3dSecureOk = "1";
		public const string AutorizarSinImportarAutenticacion3dSecure = "2";

	}

	#endregion


}
