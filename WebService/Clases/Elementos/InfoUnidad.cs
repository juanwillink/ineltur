using System;
using Ineltur.Datos;
using Ineltur.Datos.Entidades;

namespace Ineltur.WebService
{
    public class InfoUnidad
    {
        public Guid IdUnidad { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreUnidad { get; set; }
        public int Personas { get; set; }
        public int Camas { get; set; }
        public int Disponibles { get; set; }
        public decimal MontoPorUnidad { get; set; }
        public int Desayuno { get; set; }
        public int Tarifa { get; set; }
        public bool TienePromocionNxM { get; set; }
        public int? DiasACobrar { get; set; }
        public bool TienePromocionMinimoMaximo { get; set; }
        public int? MinimoDias { get; set; }
        public int? MaximoDias { get; set; }
    }
    public class InfoUnidadReservada
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? Cantidad { get; set; }
        public string NombreHabitacion { get; set; }
        public float Monto { get; set; }
        public float? MontoTotal { get; set; }
        public Guid IdUnidad { get; set; }

    }

    public class InfoUnidadConCupos
    {
        public Guid IdUnidad { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreUnidad { get; set; }
        public int Personas { get; set; }
        public int Camas { get; set; }
        public int Disponibles { get; set; }
        public decimal MontoPorUnidad { get; set; }
        public string Descripcion { get; set; }
        public CupoUnidades[] Cupos { get; set; }
        public Ineltur.Datos.Moneda? Moneda { get; set; }
    }

    public class CupoUnidades
    {
        public bool Activo { get; set; }
        public bool BloaqueadoPorPromo { get; set; }
        public int Cupomaximo { get; set; }
        public int CupoReservado { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Fecha_Alta { get; set; }
        public Guid IdCupoUnidad { get; set; }
        public Guid IdUnidadAloj { get; set; }
        public int? MarcaTemporada { get; set; }
        public decimal? Monto { get; set; }
        public decimal? MontoExtranjero { get; set; }
        public decimal? MontoMercosur { get; set; }
        public decimal? MontoExtranjeroCDTR { get; set; }
        public decimal? MontoMercosurCDTR { get; set; }
        public decimal? MontoArgentinoSDTR { get; set; }
        public decimal? MontoExtranjeroSDTR { get; set; }
        public decimal? MontoMercosurSDTR { get; set; }
        public decimal? MontoArgentinoCDTNR { get; set; }
        public decimal? MontoExtrajeroCDTNR { get; set; }
        public decimal? MontoMercosurCDTNR { get; set; }
        public decimal? MontoArgentinoSDTNR { get; set; }
        public decimal? MontoExtranjeroSDTNR { get; set; }
        public decimal? MontoMercosurSDTNR { get; set; }
    }
}