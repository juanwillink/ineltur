﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ineltur.Datos.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.6.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.2.136,1433;Initial Catalog=Turismo-ProduccionNEW;Persist Secu" +
            "rity Info=True;User ID=sa;Password=ntlvad")]
        public string Turismo_ProduccionNEWConnectionString {
            get {
                return ((string)(this["Turismo_ProduccionNEWConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.2.136,1433;Initial Catalog=Turismo-ProduccionNEW;Persist Secu" +
            "rity Info=True;User ID=sa")]
        public string Turismo_ProduccionNEWConnectionString1 {
            get {
                return ((string)(this["Turismo_ProduccionNEWConnectionString1"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.2.136;Initial Catalog=Turismo-ProduccionNEW;Integrated Securi" +
            "ty=True;User ID=sa")]
        public string Turismo_ProduccionNEWConnectionString2 {
            get {
                return ((string)(this["Turismo_ProduccionNEWConnectionString2"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.2.136;Initial Catalog=Turismo-ProduccionV3;Integrated Securit" +
            "y=True;User ID=sa")]
        public string Turismo_ProduccionV3ConnectionString {
            get {
                return ((string)(this["Turismo_ProduccionV3ConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=test.argentinahtl.com,1433;Initial Catalog=Turismo-ProduccionV3;Persi" +
            "st Security Info=True;User ID=itiers")]
        public string Turismo_ProduccionV3ConnectionString1 {
            get {
                return ((string)(this["Turismo_ProduccionV3ConnectionString1"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
		//[global::System.Configuration.DefaultSettingValueAttribute("Data Source=(local);Initial Catalog=Argentinahtl;Integrated Security=False;User I" +
		//	"D=sa")]
		[global::System.Configuration.DefaultSettingValueAttribute("Data Source=.;Server=test.argentinahtl.com,1433;Initial Catalog=Turismo-ProduccionV3;Integrated Security=False;User I" +
			"D=itiers;Password=1nELTUR13;")]
		public string ArgentinahtlConnectionString {
            get {
                return ((string)(this["ArgentinahtlConnectionString"]));
            }
        }
    }
}
