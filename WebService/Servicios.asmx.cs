﻿using Ineltur.Datos.Entidades;
using Ineltur.Datos;
using Ineltur.WebService.Clases;
using System;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
using System.Web.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ineltur.WebService
{
    [WebService(Name = "ServiciosSoap", Namespace = "http://www.ineltur.com/")]
    //[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1, Name = "BindingHotelesSoap", Location = "Hoteles.wsdl", Namespace = "http://www.ineltur.com/")]
    [System.ComponentModel.ToolboxItem(false)]
    public partial class Servicios : System.Web.Services.WebService
    {
        private static readonly Dictionary<Datos.TipoAlojamiento, Guid> TiposAlojamiento;
        private static readonly Dictionary<Guid, Datos.TipoAlojamiento> TiposAlojamiento2;
        private static readonly Dictionary<Guid, Datos.CategoriaAlojamiento> CategoriasAlojamiento;
        private static readonly Dictionary<Guid, Datos.Moneda> Monedas;

        private static readonly Guid FormaPago = Guid.Parse(Config.LeerSetting("FormaPago"));
        private static readonly Guid TipoUsuario = Guid.Parse(Config.LeerSetting("TipoUsuario"));
        private static readonly Guid Pasaporte = Guid.Parse(Config.LeerSetting("Pasaporte"));
        private static readonly Guid WebServiceSitio = Guid.Parse(Config.LeerSetting("WebServiceSitio"));
        private static readonly Guid WebServiceUsuario = Guid.Parse(Config.LeerSetting("WebServiceUsuario"));
        private static readonly Guid DNI = Guid.Parse(Config.LeerSetting("DNI"));
        private static readonly Guid CUIT = Guid.Parse(Config.LeerSetting("CUIT"));
        private static readonly long TicksOffset = new DateTime(2000, 1, 1).Ticks;
        private static readonly TimeSpan UnDia = TimeSpan.FromDays(1.0);

        private static object locker = new object();
        private static long _operationNumber = 0;

        public static long GetCurrentOperationNumber()
        {
            lock (locker)
            {
                return _operationNumber;
            }
        }

        public static long IncreaseOperationNumber()
        {
            lock (locker)
            {
                _operationNumber++;
                return _operationNumber;
            }
        }
        static Servicios()
        {
            using (var dc = NuevoDataContext())
            {
                string[] valoresEnum = Enum.GetNames(typeof(Datos.TipoAlojamiento));

                TiposAlojamiento = new Dictionary<Datos.TipoAlojamiento, Guid>();
                CategoriasAlojamiento = new Dictionary<Guid, Datos.CategoriaAlojamiento>();
                Monedas = new Dictionary<Guid, Datos.Moneda>();

                #region TiposAlojamiento

                foreach (var tipo in dc.TiposAlojamiento)
                {
                    var tipoAloj = Datos.TipoAlojamiento.SinTipoAlojamiento;

                    var tipoNombre = tipo.Nombre.Trim().ToLower();

                    tipoNombre = tipoNombre.Replace("á", "a");
                    tipoNombre = tipoNombre.Replace("é", "e");
                    tipoNombre = tipoNombre.Replace("í", "i");
                    tipoNombre = tipoNombre.Replace("ó", "o");
                    tipoNombre = tipoNombre.Replace("ú", "u");

                    if (tipoNombre.StartsWith("apart"))
                        tipoAloj = Datos.TipoAlojamiento.ApartHotel;
                    else if (tipoNombre.EndsWith("boutique"))
                        tipoAloj = Datos.TipoAlojamiento.HotelBoutique;
                    else if (tipoNombre.EndsWith("spa"))
                        tipoAloj = Datos.TipoAlojamiento.HotelSpa;
                    else if (tipoNombre.StartsWith("complejo"))
                        tipoAloj = Datos.TipoAlojamiento.ComplejoTuristico;
                    else if (tipoNombre.StartsWith("estancia"))
                        tipoAloj = Datos.TipoAlojamiento.EstanciaRanches;
                    else
                    {
                        switch (tipoNombre)
                        {
                            case "b&b": tipoAloj = Datos.TipoAlojamiento.BandB; break;
                            case "bodega": tipoAloj = Datos.TipoAlojamiento.Bodega; break;
                            case "bungalows": tipoAloj = Datos.TipoAlojamiento.Bungalows; break;
                            case "cabañas": tipoAloj = Datos.TipoAlojamiento.Cabanyas; break;
                            case "hospedaje": tipoAloj = Datos.TipoAlojamiento.Hospedaje; break;
                            case "hostal": tipoAloj = Datos.TipoAlojamiento.Hostal; break;
                            case "hosteria": tipoAloj = Datos.TipoAlojamiento.Hosteria; break;
                            case "hotel": tipoAloj = Datos.TipoAlojamiento.Hotel; break;
                            case "lodge": tipoAloj = Datos.TipoAlojamiento.Lodge; break;
                            case "motel": tipoAloj = Datos.TipoAlojamiento.Motel; break;
                            case "parador": tipoAloj = Datos.TipoAlojamiento.Parador; break;
                            case "posada": tipoAloj = Datos.TipoAlojamiento.Posada; break;
                            case "resort": tipoAloj = Datos.TipoAlojamiento.Resort; break;
                        }
                    }

                    if (!TiposAlojamiento.ContainsKey(tipoAloj))
                        TiposAlojamiento.Add(tipoAloj, tipo.IdTipoAlojamiento);
                }

                // Lo siguiente no funciona porque los nombres no pueden ser matchados correctamente
                //foreach (var tipo in dc.TiposAlojamiento)
                //{
                //    try
                //    {
                //        TiposAlojamiento.Add((Datos.TipoAlojamiento)Enum.Parse(typeof(Datos.TipoAlojamiento), tipo.Nombre.Trim(), true), tipo.IdTipoAlojamiento);
                //    }
                //    catch (Exception) { }
                //}
                #endregion

                TiposAlojamiento2 = TiposAlojamiento.ToDictionary(p => p.Value, p => p.Key);

                foreach (var categoriaAlojamiento in dc.TiposEstrellasAlojamiento)
                {
                    Datos.CategoriaAlojamiento cat = Datos.CategoriaAlojamiento.SinCategoria;

                    if (categoriaAlojamiento.Nombre.Trim().StartsWith("Otra"))
                    {
                        cat = Datos.CategoriaAlojamiento.OtraCategoria;
                    }
                    else
                    {
                        int estrellas;

                        if (Int32.TryParse(Regex.Match(categoriaAlojamiento.Nombre, @"\d+").Value,
                                out estrellas))
                        {
                            cat = (Datos.CategoriaAlojamiento)estrellas;
                        }
                    }

                    if (!CategoriasAlojamiento.ContainsKey(categoriaAlojamiento.IdTipoEstrellasAlojamiento))
                        CategoriasAlojamiento.Add(categoriaAlojamiento.IdTipoEstrellasAlojamiento, cat);
                }

                foreach (var moneda in dc.Monedas)
                {
                    try
                    {
                        Datos.Moneda m = (Datos.Moneda)Enum.Parse(typeof(Datos.Moneda), moneda.Descripcion);

                        Monedas.Add(moneda.IdMoneda, m);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static WebServiceDataContext NuevoDataContext()
        {
            var dc = new WebServiceDataContext(ConfigurationManager.ConnectionStrings["TurismoConnectionString"].ConnectionString);
            var dl = new DataLoadOptions();

            dl.LoadWith<Ciudad>(c => c.Provincia);

            dc.LoadOptions = dl;
            return dc;
        }


        private Usuario ValidarUsuarioClave(WebServiceDataContext dc, PeticionBase peticion)
        {
            var dc2 = dc;

            try
            {
                if (dc2 == null) dc2 = NuevoDataContext();

                Usuario user = dc2.Usuarios.Where(u => u.NombreUsuario == peticion.Usuario &&
                        u.Clave == peticion.Clave && u.Activo == true).SingleOrDefault();

                return user;
            }
            finally
            {
                if (dc == null) dc2.Dispose();
            }
        }

        [WebMethod]
        public RespuestaLogin Login(PeticionLogin peticion)
        {
            Usuario user;

            var operationNumber = IncreaseOperationNumber().ToString().PadLeft(8, '0');

            try
            {
                using (var dc = NuevoDataContext())
                {
                    user = ValidarUsuarioClave(dc, peticion);
                }

                if (user != null)
                {
                    return new RespuestaLogin()
                    {
                        Nombre = user.Nombre,
                        Apellido = user.Apellido,
                        User = user.NombreUsuario,
                        UID = user.IdUsuario.ToString(),
                        Estado = EstadoRespuesta.Ok
                    };
                }
                else
                {
                    return new RespuestaLogin()
                    {
                        Estado = EstadoRespuesta.NoEncontrado
                    };
                }
            }
            catch (Exception ex)
            {
                Tracker.WriteTrace(operationNumber, "Error en Login: " + ex.Message, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Stack Trace: " + ex.StackTrace, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Inner Exception: " + ex.InnerException.Message, traceType: Tracker.TraceType.Fatal);

                return null;
            }
        }


        [WebMethod]
        public RespuestaBuscarMisReservas BuscarMisReservas(PeticionBuscarMisReservas peticion)
        {
            var operationNumber = IncreaseOperationNumber().ToString().PadLeft(8, '0');

            try
            {
                using (var dc = NuevoDataContext())
                {
                    if (peticion.SearchParameter == "busqCodigo")
                    {
                        var transacciones = dc.getTransaccionesPorIdUsuarioCodigoReserva(peticion.UserId, peticion.ReservationCode);
                        var misReservas = transacciones.Select(a => new Reserva()
                        {
                            CodigoReserva = a.CODIGO_RESERVA,
                            Descripcion = a.DESCRIPCION,
                            EstadoPago = a.ESTADOPAGO,
                            EstadoReserva = a.ESTADORESERVA,
                            FechaAlta = a.FECHA_ALTA,
                            FechaVencimiento = a.FECHA_VENCIMIENTO,
                            IdAlojamiento = a.IDALOJ,
                            IdFormaDePago = a.IDFORMAPAGO,
                            IdMoneda = a.IDMONEDA,
                            IdPu = a.IDPU,
                            IdSitioOrigen = a.IDSITIOORIGEN,
                            IdTipoFormaDePago = a.IDTIPOFORMAPAGO,
                            IdTransaccion = a.IDTRANSACCION,
                            IdUsuario = a.IDUSUARIO,
                            MontoTotalConDescuento = a.MONTOTOTALCONDESC,
                            MontoTotalSinDescuento = a.MONTOTOTALSINDESC,
                            NombreAlojamiento = a.nombreAlojamiento,
                            NombreFormaDePago = a.nombreFormaPago,
                            NombrePasajero = a.pasajero,
                            TipoTransaccion = a.TIPOTRANSACCION
                        }).ToArray();
                        foreach (var reserva in misReservas)
                        {
                            var unidadesDeReserva = dc.getReservas_UnidadesTransaccionesPorIdTransaccion(reserva.IdTransaccion);
                            reserva.Unidades = unidadesDeReserva.Select(urs => new InfoUnidadReservada()
                            {
                                Cantidad = urs.cantidad,
                                FechaInicio = urs.fechaInicial,
                                FechaFin = urs.fechaFinal,
                                Monto = urs.monto,
                                MontoTotal = urs.montoTotal,
                                NombreHabitacion = urs.nombre,
                                IdUnidad = urs.idUnidad_Aloj
                            }).ToArray();

                            //var pagoNPS = dc.PAGO_NPS.Where(p => p.IdTransaccion == reserva.IdTransaccion).SingleOrDefault();

                            //if (pagoNPS != null)
                            //{
                            //    var estadoPagoNPS = dc.ESTADO_PAGO_NPS.Where(e => e.IdEstadoNPS == pagoNPS.IdEstadoNPS).SingleOrDefault();
                            //    reserva.NombreEstadoPagoNPS = estadoPagoNPS.Descripcion;
                            //}

                        }
                        var respuesta = new RespuestaBuscarMisReservas()
                        {
                            Estado = EstadoRespuesta.Ok,
                            MensajeEstado = "Reservas obtenidas correctamente",
                            MisReservas = misReservas
                        };
                        return respuesta;
                    }
                    else if (peticion.SearchParameter == "busqHistoricas")
                    {
                        var transacciones = dc.getTransaccionesPorIdUsuario(peticion.UserId);
                        var misReservas = transacciones.Select(a => new Reserva()
                        {
                            CodigoReserva = a.CODIGO_RESERVA,
                            Descripcion = a.DESCRIPCION,
                            EstadoPago = a.ESTADOPAGO,
                            EstadoReserva = a.ESTADORESERVA,
                            FechaAlta = a.FECHA_ALTA,
                            FechaVencimiento = a.FECHA_VENCIMIENTO,
                            IdAlojamiento = a.IDALOJ,
                            IdFormaDePago = a.IDFORMAPAGO,
                            IdMoneda = a.IDMONEDA,
                            IdPu = a.IDPU,
                            IdSitioOrigen = a.IDSITIOORIGEN,
                            IdTipoFormaDePago = a.IDTIPOFORMAPAGO,
                            IdTransaccion = a.IDTRANSACCION,
                            IdUsuario = a.IDUSUARIO,
                            MontoTotalConDescuento = a.MONTOTOTALCONDESC,
                            MontoTotalSinDescuento = a.MONTOTOTALSINDESC,
                            NombreAlojamiento = a.nombreAlojamiento,
                            NombreFormaDePago = a.nombreFormaPago,
                            NombrePasajero = a.pasajero,
                            TipoTransaccion = a.TIPOTRANSACCION
                        }).ToArray();
                        foreach (var reserva in misReservas)
                        {
                            var unidadesDeReserva = dc.getReservas_UnidadesTransaccionesPorIdTransaccion(reserva.IdTransaccion);
                            reserva.Unidades = unidadesDeReserva.Select(urs => new InfoUnidadReservada()
                            {
                                Cantidad = urs.cantidad,
                                FechaInicio = urs.fechaInicial,
                                FechaFin = urs.fechaFinal,
                                Monto = urs.monto,
                                MontoTotal = urs.montoTotal,
                                NombreHabitacion = urs.nombre,
                                IdUnidad = urs.idUnidad_Aloj
                            }).ToArray();

                            //var pagoNPS = dc.PAGO_NPS.FirstOrDefault(p => p.IdTransaccion == reserva.IdTransaccion);

                            //if (pagoNPS != null)
                            //{
                            //    var estadoPagoNPS = dc.ESTADO_PAGO_NPS.Where(e => e.IdEstadoNPS == pagoNPS.IdEstadoNPS).SingleOrDefault();
                            //    reserva.NombreEstadoPagoNPS = estadoPagoNPS.Descripcion;
                            //}
                        }
                        var respuesta = new RespuestaBuscarMisReservas()
                        {
                            Estado = EstadoRespuesta.Ok,
                            MensajeEstado = "Mis Reservas obtenidas correctamente",
                            MisReservas = misReservas
                        };
                        return respuesta;
                    }
                    else if (peticion.SearchParameter == "busqNuevas")
                    {
                        var transacciones = dc.getTransaccionesNuevasPorIdUsuario(peticion.UserId);
                        var misReservas = transacciones.Select(a => new Reserva()
                        {
                            CodigoReserva = a.CODIGO_RESERVA,
                            Descripcion = a.DESCRIPCION,
                            EstadoPago = a.ESTADOPAGO,
                            EstadoReserva = a.ESTADORESERVA,
                            FechaAlta = a.FECHA_ALTA,
                            FechaVencimiento = a.FECHA_VENCIMIENTO,
                            IdAlojamiento = a.IDALOJ,
                            IdFormaDePago = a.IDFORMAPAGO,
                            IdMoneda = a.IDMONEDA,
                            IdPu = a.IDPU,
                            IdSitioOrigen = a.IDSITIOORIGEN,
                            IdTipoFormaDePago = a.IDTIPOFORMAPAGO,
                            IdTransaccion = a.IDTRANSACCION,
                            IdUsuario = a.IDUSUARIO,
                            MontoTotalConDescuento = a.MONTOTOTALCONDESC,
                            MontoTotalSinDescuento = a.MONTOTOTALSINDESC,
                            NombreAlojamiento = a.nombreAlojamiento,
                            NombreFormaDePago = a.nombreFormaPago,
                            NombrePasajero = a.pasajero,
                            TipoTransaccion = a.TIPOTRANSACCION
                        }).ToArray();

                        foreach (var reserva in misReservas)
                        {
                            var unidadesDeReserva = dc.getReservas_UnidadesTransaccionesPorIdTransaccion(reserva.IdTransaccion);
                            reserva.Unidades = unidadesDeReserva.Select(urs => new InfoUnidadReservada()
                            {
                                Cantidad = urs.cantidad,
                                FechaInicio = urs.fechaInicial,
                                FechaFin = urs.fechaFinal,
                                Monto = urs.monto,
                                MontoTotal = urs.montoTotal,
                                NombreHabitacion = urs.nombre,
                                IdUnidad = urs.idUnidad_Aloj
                            }).ToArray();

                            //var pagoNPS = dc.PAGO_NPS.Where(p => p.IdTransaccion == reserva.IdTransaccion).SingleOrDefault();

                            //if (pagoNPS != null)
                            //{
                            //    var estadoPagoNPS = dc.ESTADO_PAGO_NPS.Where(e => e.IdEstadoNPS == pagoNPS.IdEstadoNPS).SingleOrDefault();
                            //    reserva.NombreEstadoPagoNPS = estadoPagoNPS.Descripcion;
                            //}
                        }
                        var respuesta = new RespuestaBuscarMisReservas()
                        {
                            Estado = EstadoRespuesta.Ok,
                            MensajeEstado = "Mis Reservas obtenidas correctamente",
                            MisReservas = misReservas
                        };
                        return respuesta;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracker.WriteTrace(operationNumber, "Error en Login: " + ex.Message, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Stack Trace: " + ex.StackTrace, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Inner Exception: " + ex.InnerException.Message, traceType: Tracker.TraceType.Fatal);

                return null;
            }
        }

        [WebMethod]
        public RespuestaInfoCuposSemanalesAlojamiento BuscarPreciosSemanalesHotel(PeticionInfoAlojamiento peticion)
        {
            var operationNumber = IncreaseOperationNumber().ToString().PadLeft(8, '0');
            try
            {
                using (var dc = NuevoDataContext())
                {
                    var usuario = ValidarUsuarioClave(dc, peticion);
                    if (usuario == null)
                    {
                        var respuesta = new RespuestaInfoCuposSemanalesAlojamiento()
                        {
                            Estado = EstadoRespuesta.ErrorParametro,
                            MensajeEstado = "No se pudo validar el usuario"
                        };
                        return respuesta;
                    }
                    var markup = 1 / ((1 - (usuario.MarkupAAgencia) / 100) * (1 - (usuario.MarkupAConsumidorFinal) / 100));
                    var monedaAloj = ObtenerMoneda(dc, (Guid)peticion.IdAlojamiento);
                    var cotizacionUsuario = ObtenerCotizacion(dc, usuario.IdMoneda);


                    var rsAlojamiento = dc.Alojamientos.SingleOrDefault(a => a.IdAlojamiento == peticion.IdAlojamiento);
                    if (peticion.IdAlojamiento != null)
                    {

                        var cotizacionAloj = ObtenerCotizacionTarifaAlojamiento(dc, rsAlojamiento.IdAlojamiento, peticion.Nationality);

                        var unidades = dc.getUnidades_AlojPorIdAloj(peticion.IdAlojamiento);
                        var unidadesAlojamiento = unidades.Select(uni => new InfoUnidadConCupos()
                        {
                            NombreUnidad = uni.NOMBRE,
                            Camas = uni.CANTCAMAS,
                            Personas = uni.CANTPERSONAS,
                            IdUnidad = uni.IDUNIDAD_ALOJ,
                            Descripcion = uni.DESCRIPCION,
                            Moneda = ConvertirMoneda(usuario.IdMoneda)
                        }).ToArray();
                        foreach (var unidad in unidadesAlojamiento)
                        {
                            var rsCupo = dc.getCuposUnidadesPorFecha(unidad.IdUnidad, peticion.Fecha, 's');
                            unidad.Cupos = rsCupo.Select(cupo => new CupoUnidades()
                            {
                                Activo = cupo.ACTIVO,
                                BloaqueadoPorPromo = cupo.BLOQUEADOPORPROMO,
                                Cupomaximo = cupo.CUPOMAXIMO,
                                CupoReservado = cupo.CUPORESERVADO,
                                Fecha = cupo.FECHA,
                                Fecha_Alta = cupo.FECHA_ALTA,
                                IdCupoUnidad = cupo.IDCUPOUNIDAD,
                                IdUnidadAloj = cupo.IDUNIDAD_ALOJ,
                                MarcaTemporada = cupo.MARCA_TEMPORADA,
                                Monto = (float.IsNaN(cupo.MONTO)) ? 0 : Decimal.Round((decimal)((cupo.MONTO * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoExtranjero = (cupo.MONTO_EXT == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_EXT * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoMercosur = (cupo.MONTO_MER == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_MER * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoExtranjeroCDTR = (cupo.MONTO_EXT_CD_TR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_EXT_CD_TR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoMercosurCDTR = (cupo.MONTO_MER_CD_TR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_MER_CD_TR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoArgentinoCDTNR = (cupo.MONTO_RA_CD_TNR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_RA_CD_TNR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoExtrajeroCDTNR = (cupo.MONTO_MER_CD_TNR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_MER_CD_TNR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoMercosurCDTNR = (cupo.MONTO_MER_CD_TNR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_MER_CD_TNR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoArgentinoSDTR = (cupo.MONTO_RA_SD_TR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_RA_SD_TR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoExtranjeroSDTR = (cupo.MONTO_EXT_SD_TR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_EXT_SD_TR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoMercosurSDTR = (cupo.MONTO_MER_SD_TR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_MER_SD_TR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoArgentinoSDTNR = (cupo.MONTO_RA_SD_TNR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_RA_SD_TNR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoExtranjeroSDTNR = (cupo.MONTO_EXT_SD_TNR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_EXT_SD_TNR * markup) * (cotizacionAloj / cotizacionUsuario)), 0),
                                MontoMercosurSDTNR = (cupo.MONTO_MER_SD_TNR == null) ? 0 : Decimal.Round((decimal)((cupo.MONTO_MER_SD_TNR * markup) * (cotizacionAloj / cotizacionUsuario)), 0)
                            }).ToArray();
                        }
                        var alojamiento = new InfoCuposAlojamiento()
                        {
                            Direccion = rsAlojamiento.Direccion,
                            Nombre = rsAlojamiento.Nombre,
                            Unidades = unidadesAlojamiento                            
                        };
                        var respuesta = new RespuestaInfoCuposSemanalesAlojamiento()
                        {
                            Estado = EstadoRespuesta.Ok,
                            Alojamiento = alojamiento
                        };
                        return respuesta;
                    }
                    else
                    {
                        var respuesta = new RespuestaInfoCuposSemanalesAlojamiento()
                        {
                            Estado = EstadoRespuesta.ErrorParametro,
                            MensajeEstado = "Id alojamiento no puede ser null"
                        };
                        return respuesta;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracker.WriteTrace(operationNumber, "Error en Login: " + ex.Message, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Stack Trace: " + ex.StackTrace, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Inner Exception: " + ex.InnerException.Message, traceType: Tracker.TraceType.Fatal);

                return null;
            }

        }

        [WebMethod]
        public RespuestaBuscarAlojamientosEInfo SearchHotels(PeticionBuscarAlojamientos petition)
        {
            var operationNumber = IncreaseOperationNumber().ToString().PadLeft(8, '0');

            try
            {
                using (var dc = NuevoDataContext())
                {
                    var usuario = ValidarUsuarioClave(dc, petition);
                    if (usuario == null)
                    {
                        var respuestaError = new RespuestaBuscarAlojamientosEInfo()
                        {
                            Estado = EstadoRespuesta.OperacionFallida,
                            MensajeEstado = "Usuario no validado"
                        };
                        return respuestaError;
                    }
                    Guid? ciudad = null;
                    Guid? provincia = null;

                    if (petition.IdDestino.ToString() == "00000000-0000-0000-0000-000000000000")
                    {
                        petition.IdDestino = dc.Alojamientos.Where(a => a.Nombre == petition.NombreAlojamiento).Select(a => a.IdCiudad).Single();
                    }

                    switch (petition.TipoDestino)
                    {
                        case TipoDestino.Ciudad:
                            ciudad = petition.IdDestino;
                            break;

                        case TipoDestino.Provincia:
                            provincia = petition.IdDestino;
                            break;
                        case TipoDestino.NoEspecificado:
                            ciudad = petition.IdDestino;
                            break;
                    }
                    if (petition.FechaInicio == DateTime.MinValue)
                    {
                        //Se busco sin fecha de checkin
                        var hoteles = dc.getAlojamientosConfirmadosPorIdCiudad(GetOrderString(petition.Orden), ciudad, null, null, petition.NombreAlojamiento).ToArray();

                        if (hoteles.Length == 0)
                        {
                            var respuestaError = new RespuestaBuscarAlojamientosEInfo()
                            {
                                Estado = EstadoRespuesta.NoEncontrado
                            };

                            Tracker.WriteTrace(operationNumber, "Fin BuscarAlojamientosEInfo: " + respuestaError.ToStringWithProperties(), traceType: Tracker.TraceType.Error);

                            return respuestaError;
                        }

                        var markup = 1 / ((1 - (usuario.MarkupAAgencia) / 100) * (1 - (usuario.MarkupAConsumidorFinal) / 100));

                        var cotizMonedaUsuario = ObtenerCotizacion(dc, usuario.IdMoneda);

                        var alojamientos = hoteles.Select(a => new InfoAlojamientoDisponibleCompleta()
                        {
                            Destino = new InfoDestino()
                            {
                                TipoDestino = TipoDestino.Ciudad,
                                IdDestino = a.idCiudad.GetValueOrDefault(),
                                NombreDestino = string.Concat(a.NOMBRECIUDAD, ", ", a.NOMBREPROVINCIA)
                            },
                            Alojamiento = ObtenerInfoAlojamiento(dc.Alojamientos.SingleOrDefault(aloj => aloj.IdAlojamiento == a.idAloj), usuario.IdMoneda),
                            TipoAlojamiento = ConvertirTipoAlojamiento(a.idTipoAloj),
                            IdAlojamiento = a.idAloj.GetValueOrDefault(),
                            Nombre = a.nombre,
                            Moneda = ConvertirMoneda(usuario.IdMoneda),
                            MontoUnidadMasBarata = (a.montoUnidadMasBarata.GetValueOrDefault() * markup) * (ObtenerCotizacionTarifaAlojamiento(dc, a.idAloj.GetValueOrDefault(), petition.Nacionalidad) / cotizMonedaUsuario)
                        }).ToArray();

                        var respuesta = new RespuestaBuscarAlojamientosEInfo()
                        {
                            AlojamientosDisponibles = alojamientos,
                            Estado = EstadoRespuesta.Ok,
                            MensajeEstado = "Se encontraron todos los Hoteles"
                        };
                        return respuesta;
                    }
                    else
                    {
                        //Se busco con fecha de Checkin



                        var hoteles = dc.getTarifasTodasDeUnidadPedida(
                            petition.Habitacion1.GetValueOrDefault(), petition.Habitacion2.GetValueOrDefault(),
                            petition.Habitacion3.GetValueOrDefault(), petition.Habitacion4.GetValueOrDefault(),
                            petition.Habitacion5.GetValueOrDefault(), petition.Habitacion6.GetValueOrDefault(),
                            petition.FechaInicio, petition.FechaFin, ConvertirOrdenamiento(petition.Orden),
                            ciudad, provincia, ConvertirTipoAlojamiento(petition.TipoAlojamiento), petition.Nacionalidad,
                            petition.NombreAlojamiento, null, null, usuario.IdUsuario)
                                .ToArray();
                                




                        //var hoteles = dc.GetAlojamientosConDisponibilidadV4(
                        //    petition.Habitacion1.GetValueOrDefault(), petition.Habitacion2.GetValueOrDefault(),
                        //petition.Habitacion3.GetValueOrDefault(), petition.Habitacion4.GetValueOrDefault(),
                        //petition.Habitacion5.GetValueOrDefault(), petition.Habitacion6.GetValueOrDefault(),
                        //petition.FechaInicio, petition.FechaFin, ConvertirOrdenamiento(petition.Orden),
                        //ciudad, provincia, ConvertirTipoAlojamiento(petition.TipoAlojamiento), petition.Nacionalidad, petition.NombreAlojamiento, petition.desayuno, petition.tarifaReembolsable, usuario.IdUsuario)
                        //    .Where(a => a.montoTotalEstimadoEnPesos > 0).ToArray();

                        if (hoteles.Length == 0)
                        {
                            var respuestaError = new RespuestaBuscarAlojamientosEInfo()
                            {
                                Estado = EstadoRespuesta.NoEncontrado
                            };

                            Tracker.WriteTrace(operationNumber, "Fin BuscarAlojamientosEInfo: " + respuestaError.ToStringWithProperties(), traceType: Tracker.TraceType.Error);

                            return respuestaError;
                        }
                        else
                        {
                            var markup = 1 / ((1 - (usuario.MarkupAAgencia) / 100) * (1 - (usuario.MarkupAConsumidorFinal) / 100));

                            var cotizMonedaUsuario = ObtenerCotizacion(dc, usuario.IdMoneda); //Agregado por AM

                            Ciudad nombreCiudad = new Ciudad();
                            Provincia nombreProvincia = new Provincia();

                            if (petition.TipoDestino == TipoDestino.Ciudad)
                            {
                                nombreCiudad = dc.Ciudades.Where(c => c.IdCiudad == ciudad).SingleOrDefault();
                                nombreProvincia = dc.Provincias.Where(p => p.IdProvincia == nombreCiudad.IdProvincia).SingleOrDefault();
                            }
                            else
                            {
                                nombreCiudad.Nombre = "";
                                nombreProvincia = dc.Provincias.Where(p => p.IdProvincia == petition.IdDestino).SingleOrDefault();
                            }

                            var alojamientosDisponibles = hoteles.Select(a => new InfoAlojamientoDisponibleCompleta()
                            {

                                Destino = new InfoDestino()
                                {
                                    TipoDestino = TipoDestino.Ciudad,
                                    IdDestino = ciudad.GetValueOrDefault(),
                                    NombreDestino = String.Concat(nombreCiudad.Nombre, ", ", nombreProvincia.Nombre)
                                },

                                Alojamiento = ObtenerInfoAlojamiento(dc.Alojamientos.SingleOrDefault(aloj => aloj.IdAlojamiento == a.idAloj), usuario.IdMoneda),

                                IdAlojamiento = a.idAloj.GetValueOrDefault(),
                                Nombre = a.nombreAlojamiento,
                                Moneda = ConvertirMoneda(usuario.IdMoneda),

                                Cupo1 = a.cupoDisponible,

                                
                                
                                Tarifa1 = Decimal.Round((decimal)((ElegirTarifaMasBarata(a, petition.Nacionalidad) * markup) * (ObtenerCotizacionTarifaAlojamiento(dc, a.idAloj.GetValueOrDefault(), petition.Nacionalidad) / cotizMonedaUsuario)), 2),
                               
                            }).ToArray();

                            alojamientosDisponibles = alojamientosDisponibles.OrderBy(a => a.Tarifa1).GroupBy(a => a.IdAlojamiento).Select(a => a.First()).ToArray();
                            alojamientosDisponibles = alojamientosDisponibles.OrderBy(a => a.Alojamiento.Nombre).ToArray();

                            foreach (var alojamientoDisponible in alojamientosDisponibles)
                            {

                                var monedaAloj = ObtenerMoneda(dc, (Guid)alojamientoDisponible.IdAlojamiento);
                                var cotizMonedaAloj = ObtenerCotizacionTarifaAlojamiento(dc, alojamientoDisponible.IdAlojamiento, petition.Nacionalidad);

                                alojamientoDisponible.Alojamiento.Unidades = hoteles.Where(d => d.cupoDisponible.GetValueOrDefault() > 0 && d.idAloj == alojamientoDisponible.IdAlojamiento)
                                    .Select(d => new InfoUnidad()
                                    {
                                        IdUnidad = d.idUnidadAloj.GetValueOrDefault(),
                                        //Fecha = d.FECHA,
                                        //Tarifa = petition.tarifaReembolsable,
                                        //Desayuno = petition.desayuno,
                                        NombreUnidad = d.nombreHabitacion,
                                        Personas = d.cantPersonas.GetValueOrDefault(),
                                        Camas = d.cantCamas.GetValueOrDefault(),
                                        Disponibles = d.cupoDisponible.GetValueOrDefault(),
                                        MontoPorUnidadRaCdTr = (petition.Nacionalidad == "arg") ? Decimal.Round((decimal)((d.montoPromedioPorDia.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadRaCdTnr = (petition.Nacionalidad == "arg") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMRCDTNR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadRaSdTr = (petition.Nacionalidad == "arg") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMRSDTR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadRaSdTnr = (petition.Nacionalidad == "arg") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMRSDTNR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadExCdTr = (petition.Nacionalidad == "ext") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMECDTR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadExCdTnr = (petition.Nacionalidad == "ext") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMECDTNR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadExSdTr = (petition.Nacionalidad == "ext") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMESDTR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadExSdTnr = (petition.Nacionalidad == "ext") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMESDTNR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadMeCdTr = (petition.Nacionalidad == "mer") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMMCDTR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadMeCdTnr = (petition.Nacionalidad == "mer") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMMCDTNR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadMeSdTr = (petition.Nacionalidad == "mer") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMMSDTR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                        MontoPorUnidadMeSdTnr = (petition.Nacionalidad == "mer") ? Decimal.Round((decimal)((d.montoPromedioPorDiaMMSDTNR.GetValueOrDefault() * markup) * (cotizMonedaAloj / cotizMonedaUsuario)), 2) : 0,
                                    }).ToArray();

                                foreach (var unidad in alojamientoDisponible.Alojamiento.Unidades)
                                {
                                    var promociones = dc.PROMOCIONES_ALOJAMIENTOs.Where(p => p.IDUNIDADPROMO == unidad.IdUnidad &&
                                        p.ACTIVO == true).ToArray();

                                    if (promociones != null)
                                    {
                                        unidad.Promociones = promociones.Select(d => new PromotionModel()
                                        {
                                            Activo = d.ACTIVO,
                                            Descripcion1 = d.DESCRIPCION,
                                            Descripcion2 = d.DESCRIPCION2,
                                            Descuento = d.DESCUENTO,
                                            DiasACobrar = d.DIASACOBRAR,
                                            DiasReservados = d.DIASRESERVADOS,
                                            FechaFin = d.FECHAFIN,
                                            FechaInicio = d.FECHAINICIO,
                                            IdUnidadPromo = d.IDUNIDADPROMO,
                                            LodgingId = d.IDALOJ,
                                            MaximoNoches = d.MAXIMONOCHES,
                                            MinimoNoches = d.MINIMONOCHES,
                                            NombrePromocion = d.NOMBRE,
                                            PromocionId = d.IDPROMOCION,
                                            Slogan = d.SLOGAN,
                                            TipoPromocionId = d.IDTIPOPUBLICACIONPROMO
                                        }).ToArray();
                                    }
                                }
                            }

                            var respuesta = new RespuestaBuscarAlojamientosEInfo()
                            {
                                Estado = EstadoRespuesta.Ok,
                                AlojamientosDisponibles = alojamientosDisponibles
                            };

                            Tracker.WriteTrace(operationNumber, "Fin BuscarAlojamientosEInfo: Éxito");
                            Tracker.WriteTrace(operationNumber, "Respuesta: " + respuesta.ToStringWithProperties());

                            return respuesta;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var respuestaError = new RespuestaBuscarAlojamientosEInfo()
                {
                    Estado = EstadoRespuesta.ErrorInterno,
                    MensajeEstado = string.Empty //ex.ToString()
                };

                Tracker.WriteTrace(operationNumber, "Fin BuscarAlojamientosEInfo: " + respuestaError.ToStringWithProperties() + ex.Message, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Stack Trace: " + ex.StackTrace, traceType: Tracker.TraceType.Fatal);
                Tracker.WriteTrace(operationNumber, "Inner Exception: " + ex.InnerException.Message, traceType: Tracker.TraceType.Fatal);

                return respuestaError;
            }
        }

        private InfoAlojamiento ObtenerInfoAlojamiento(Alojamiento alojamiento, Guid idMonedaUsuario)
        {
            var info = new InfoAlojamiento();

            CargarAmenidadesRegimenAlojamiento(alojamiento, info);
            CargarInfoAlojamiento(alojamiento, info, idMonedaUsuario);

            return info;
        }

        private static void CargarInfoAlojamiento(Alojamiento alojamiento, InfoAlojamiento info, Guid idMonedaUsuario)
        {
            info.IdAlojamiento = alojamiento.IdAlojamiento;
            info.Nombre = alojamiento.Nombre;
            info.Descripcion = alojamiento.Descripcion;
            info.Descripcion2 = alojamiento.Descripcion2;
            info.Direccion = alojamiento.Direccion;
            info.Tipo = ConvertirTipoAlojamiento(alojamiento.IdTipoAlojamiento);
            info.Categoria = ConvertirCategoria(alojamiento.IdTipoEstrellaAlojamiento);
            info.PoliticasCancelacion = alojamiento.PoliticasCancelacion;
            //info.DiasCancelacionCargo = alojamiento.DiasCancelacionCargo;
            info.BajoPeticion = alojamiento.IdTipoPerfil == Guid.Parse("DDEAB0FD-6515-4788-9034-40C1888459C4");
            info.Telefono = alojamiento.Telefono;
            info.Longitud = alojamiento.Longitud;
            info.Latitud = alojamiento.Latitud;
            //info.Moneda = ConvertirMoneda(alojamiento.IdMoneda.GetValueOrDefault());//habría que enviar la moneda del usuario NO la del hotel
            info.Moneda = ConvertirMoneda(idMonedaUsuario);
        }

        private static float? ElegirTarifaMasBarata(getTarifasTodasDeUnidadPedidaResult a, string nacionalidad)
        {

            List<float?> minimizedArray = new List<float?>();
            switch (nacionalidad)
            {
                case "arg":
                    minimizedArray.Add(a.montoPromedioPorDia);
                    minimizedArray.Add(a.montoPromedioPorDiaMRCDTNR);
                    minimizedArray.Add(a.montoPromedioPorDiaMRSDTNR);
                    minimizedArray.Add(a.montoPromedioPorDiaMRSDTR);
                    break;
                case "ext":
                    minimizedArray.Add(a.montoPromedioPorDiaMECDTNR);
                    minimizedArray.Add(a.montoPromedioPorDiaMECDTR);
                    minimizedArray.Add(a.montoPromedioPorDiaMESDTNR);
                    minimizedArray.Add(a.montoPromedioPorDiaMESDTR);
                    break;
                case "mer":
                    minimizedArray.Add(a.montoPromedioPorDiaMMCDTNR);
                    minimizedArray.Add(a.montoPromedioPorDiaMMCDTR);
                    minimizedArray.Add(a.montoPromedioPorDiaMMSDTNR);
                    minimizedArray.Add(a.montoPromedioPorDiaMMSDTR);
                    break;
                default:
                    break;
            }
            float?[] arrayWithoutZeros = minimizedArray.Where(c => c != 0).ToArray();
            float? montoMenor = 0;
            if (arrayWithoutZeros.Length != 0)
            {
                montoMenor = arrayWithoutZeros.Min();
            }
            

            //float?[] array = { monto1, monto2, monto3, monto4, monto5, monto6 };
            //float? montoMenor = 0;
            //for (int i = 0; i < 4; i++)
            //{
            //    if (array[i] != null)
            //    {
            //        if (array[i + 1] != null)
            //        {
            //            if (array[i] < array[i + 1])
            //            {

            //                if (array[i] < montoMenor || montoMenor == 0)
            //                {
            //                    montoMenor = array[i];
            //                }

            //            }
            //        }
            //        else
            //        {
            //            montoMenor = array[i];
            //        }
            //    }
            //}
            return montoMenor;
        }

        private static Datos.TipoAlojamiento ConvertirTipoAlojamiento(Guid? tipo)
        {
            if (!tipo.HasValue) return Datos.TipoAlojamiento.SinTipoAlojamiento;

            Datos.TipoAlojamiento ta;

            return TiposAlojamiento2.TryGetValue(tipo.Value, out ta) ? ta : 0;
        }

        private static Guid? ConvertirTipoAlojamiento(Datos.TipoAlojamiento? tipo)
        {
            if (!tipo.HasValue) return null;

            Guid ta;

            return TiposAlojamiento.TryGetValue(tipo.Value, out ta) ? ta : (Guid?)null;
        }

        private static string ConvertirOrdenamiento(OrdenAlojamientos? orden)
        {
            switch (orden.GetValueOrDefault(ParametrosBasicos.OrdenamientoAlojamientosPorDefecto))
            {
                case OrdenAlojamientos.PorPrecio: return "precio";
                case OrdenAlojamientos.PorCategoria: return "estrellas";
                case OrdenAlojamientos.PorNombre: return "alfabetico";
            }
            return String.Empty;
        }

        private static Datos.Moneda ConvertirMoneda(Guid moneda)
        {
            Datos.Moneda mon;

            if (Monedas.TryGetValue(moneda, out mon)) return mon;
            return 0;
        }

        private static CategoriaAlojamiento ConvertirCategoria(Guid categoria)
        {
            CategoriaAlojamiento cat;

            if (CategoriasAlojamiento.TryGetValue(categoria, out cat)) return cat;
            return CategoriaAlojamiento.SinCategoria;
        }

        private static void CargarAmenidadesRegimenAlojamiento(Alojamiento alojamiento, InfoAlojamiento info)
        {
            using (var dc = NuevoDataContext())
            {
                var amenidades = alojamiento.ServicioAlojamientos.Where(sa => sa.Fijo).Join(dc.Servicios, sa => sa.IdServ, s => s.IdServ, (sa, s) => new { IdServ = s.IdServ, NombreServ = s.Nombre });
                info.Amenidades = new List<string>();
                info.Regimen = RegimenAlojamiento.NoInformado;

                foreach (var amenidad in amenidades)
                {
                    switch (amenidad.IdServ.ToString())
                    {
                        case "8e88292e-f6fe-40a2-821c-4d0fbd5ef025": // Desayuno
                            info.Regimen = RegimenAlojamiento.Desayuno;
                            break;

                        case "7f32a94a-ca84-43b4-943f-eff30340d8b7": // Media Pensión
                            info.Regimen = RegimenAlojamiento.MediaPension;
                            break;

                        case "fb0bfbc2-2baf-4c17-8dec-ae2c8030abd8": // Pensión Completa
                            info.Regimen = RegimenAlojamiento.PensionCompleta;
                            break;

                        default:
                            info.Amenidades.Add(amenidad.NombreServ); break;
                    }
                }
            }
        }

        private string GetOrderString(OrdenAlojamientos? orden)
        {
            switch (orden)
            {
                case OrdenAlojamientos.PorCategoria:
                    return "estrellas";
                case OrdenAlojamientos.PorNombre:
                    return "nombre";
                case OrdenAlojamientos.PorPrecio:
                    return "precio";
                default:
                    return "";
            }
        }

        private static Guid ObtenerMoneda(WebServiceDataContext dc, Guid idAlojamiento)
        {
            var dc2 = dc;

            try
            {

                if (dc2 == null) dc2 = NuevoDataContext();

                Guid idMoneda = (Guid)dc2.Alojamientos.Where(a => a.IdAlojamiento == idAlojamiento).Select(a => a.IdMoneda).SingleOrDefault();

                return idMoneda;
            }
            finally
            {
                if (dc == null) dc2.Dispose();
            }

        }

        private static float ObtenerCotizacion(WebServiceDataContext dc, Guid idMoneda)
        {
            var dc2 = dc;

            try
            {

                if (dc2 == null) dc2 = NuevoDataContext();

                float cotizAlojamiento = dc2.Monedas.Where(m => m.IdMoneda == idMoneda).Select(m => m.Cotizacion).SingleOrDefault();

                return cotizAlojamiento;
            }
            finally
            {
                if (dc == null) dc2.Dispose();
            }

        }

        private static float ObtenerCotizacionTarifaAlojamiento(WebServiceDataContext dc, Guid? idAloj, string idNacionalidad)
        {
            var dc2 = dc;

            try
            {

                if (dc2 == null) dc2 = NuevoDataContext();

                var idMoneda = dc2.Tarifas_Alojamientos.Where(t => t.IDALOJ == idAloj && t.IDNACIONALIDAD == idNacionalidad && t.FECHA_DESDE <= DateTime.Now && t.FECHA_HASTA >= DateTime.Now).Select(t => t.IDMONEDA).SingleOrDefault();

                float cotizAlojamiento = dc2.Monedas.Where(m => m.IdMoneda == idMoneda).Select(m => m.Cotizacion).SingleOrDefault();

                return cotizAlojamiento;
            }
            finally
            {
                if (dc == null) dc2.Dispose();
            }

        }
    }
}
