using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace ArgentinahtlCommon
{
    public static class Tracker
    {
        /*
            TABLA LOG
	            IDLOG
	            TYPE
	            APPLICATION
	            PROCESS
	            THREAD
	            METHOD
	            LINE	(TYPE=ERROR/EXCEPTION)
	            DETAIL
         */
        public enum TraceType { Info, Error, Fatal }

        private static string TAB = new string(' ', 50);

        public static string SEPARATOR { get { return TAB + new string('-', 75); } }
        public static string DOUBLE_SEPARATOR { get { return new string('=', 90); } }

        //public static void WriteTrace()
        //{
        //    WriteTrace(string.Empty);
        //}

        //public static void WriteTrace(string trace)
        //{
        //    WriteTrace(TraceType.Info, trace, false);
        //}

        //public static void WriteTrace(TraceType traceType, string trace)
        //{
        //    WriteTrace(traceType, trace, false);
        //}

        //public static void WriteTrace(string trace, bool additionalNewLine)
        //{
        //    WriteTrace(TraceType.Info, trace, additionalNewLine);
        //}

        public static void WriteTrace(string trace = "", bool additionalNewLine = true, TraceType traceType = TraceType.Info)
        {
            var logger = LogManager.GetCurrentClassLogger();

            if (traceType != TraceType.Fatal)
                trace = trace.Replace("\n", "\n" + TAB);

            if (additionalNewLine)
                trace += "\n";

            LogEventInfo loggerEvent;

            switch (traceType)
            {
                case TraceType.Info:
                    loggerEvent = new LogEventInfo(LogLevel.Info, "", trace);
                    loggerEvent.Properties["LogTypeId"] = 1;
                    //loggerEvent.Properties["SessionIdentifier"] = identifier;
                    logger.Log(loggerEvent);
                    //logger.Info(trace);
                    break;
            
                case TraceType.Error:
                    loggerEvent = new LogEventInfo(LogLevel.Error, "", trace);
                    loggerEvent.Properties["LogTypeId"] = 2;
                    //loggerEvent.Properties["SessionIdentifier"] = identifier;
                    logger.Log(loggerEvent);
                    //logger.Error(trace); 
                    break;
            
                case TraceType.Fatal:
                    loggerEvent = new LogEventInfo(LogLevel.Fatal, "", trace);
                    loggerEvent.Properties["LogTypeId"] = 2;
                    //loggerEvent.Properties["SessionIdentifier"] = identifier;
                    logger.Log(loggerEvent);
                    //logger.Fatal(trace); 
                    break;
            }
        }

		public static string SerializarObjeto(Object objeto)
		{
			var encoding = Encoding.GetEncoding("iso-8859-1");

			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true,
				Encoding = encoding
			};

			try
			{

				XmlSerializer xmlSerializer = new XmlSerializer(objeto.GetType());

				using (var stream = new MemoryStream())
				{
					using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
					{
						xmlSerializer.Serialize(xmlWriter, objeto);
					}
					return encoding.GetString(stream.ToArray());
				}

			}
			catch (Exception ex)
			{
				return ex.Message + "\n" + (ex.InnerException != null ? "Inner Exception: " + ex.InnerException.Message : string.Empty);
				//try
				//{
				//    string rpta = string.Empty;
				//    rpta = objeto.GetType().ToString();

				//    return rpta;

				//}
				//catch (Exception ex)
				//{
				//    return ex.Message + "\nInner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
				//}
			}

			//El siguiente código hace lo mismo que el anterior pero con codificación UTF-16
			//XmlSerializer xmlSerializer = new XmlSerializer(objeto.GetType());
			//StringWriter textWriter = new StringWriter();
			//xmlSerializer.Serialize(textWriter, objeto);
			//return textWriter.ToString();


		}
	}
}