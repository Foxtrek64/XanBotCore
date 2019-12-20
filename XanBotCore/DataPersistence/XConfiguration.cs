using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.Logging;
using XanBotCore.ServerRepresentation;
using XanBotCore.Utility;

namespace XanBotCore.DataPersistence {

	/// <summary>
	/// A utility class that manages configuration files.
	/// </summary>
	public class XConfiguration {
		//private static readonly string CFG_FILE_NAME = "configuration.cfg";
		public static readonly XConfiguration GLOBAL_CONFIGURATION = new XConfiguration(XFileHandler.GLOBAL_HANDLER);
		private readonly Dictionary<string, string> ConfigValues = new Dictionary<string, string>();
		private static readonly Dictionary<XFileHandler, Dictionary<string, XConfiguration>> ConfigurationTies = new Dictionary<XFileHandler, Dictionary<string, XConfiguration>>();

		/// <summary>
		/// The name of this configuration file.
		/// </summary>
		public string ConfigFileName { get; } = "configuration.cfg";

		/// <summary>
		/// The underlying XFileHandler for this configuration object.
		/// </summary>
		public XFileHandler BaseHandler { get; } = null;

		/// <summary>
		/// True if the system has unsaved changes.
		/// </summary>
		private bool HasUnsavedChanges { get; set; } = false;

		private XConfiguration(XFileHandler handler, string cfgFileName = "configuration.cfg") {
			BaseHandler = handler;
			ConfigFileName = cfgFileName;
			LoadConfigurationFile();
		}


		/// <summary>
		/// Ambiguous variation of TryParse that attempts to work for any type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input"></param>
		/// <returns></returns>
		private static bool TryParse<T>(string input, out T value) {
			try {
				value = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
				return true;
			} catch {
				value = default;
				return false;
			}
		}

		/// <summary>
		/// Mandates the type of a config value by returning the value in the key as the specified type, or by writing the default value and returning the default value if the key is malformed or doesn't exist.<para/>
		/// When tying a config value into a property, this method is suggested.<para/>
		/// If you want to display an error message, consider using <see cref="GetAndMandateType{T}(string, T)"/>
		/// </summary>
		/// <typeparam name="T">The type of data that is desired.</typeparam>
		/// <param name="configKey">The config key to search.</param>
		/// <param name="defaultValue">The default value if the config key doesn't exist or if the data is malformed.</param>
		public T TryGetType<T>(string configKey, T defaultValue) {
			string v = GetConfigurationValue(configKey, defaultValue.ToString(), true, true);
			if (!TryParse(v, out T retn)) {
				retn = defaultValue;

				string defValueStr = defaultValue.ToString();
				if (defaultValue.GetType() == typeof(bool)) {
					// This is to ensure it formats properly in >> config list
					defValueStr = defValueStr.ToLower();
				}
				SetConfigurationValue(configKey, defValueStr);
			}
			return retn;
		}

		/// <summary>
		/// A stricter variation of <see cref="TryGetType{T}(string, T)"/> that throws a <see cref="MalformedConfigDataException"/> if the config value could not be cast into the target type.<para/>
		/// If the data in the config key is unable to be cast into the target type, the default value will be written.
		/// </summary>
		/// <typeparam name="T">The type of data that is desired.</typeparam>
		/// <param name="configKey">The config key to search.</param>
		/// <param name="defaultValue">The default value if the config key doesn't exist or if the data is malformed.</param>
		public void GetAndMandateType<T>(string configKey, T defaultValue, out T value) {
			string v = GetConfigurationValue(configKey, defaultValue.ToString(), true, true);
			if (!TryParse(v, out T retn)) {
				string defValueStr = defaultValue.ToString();
				if (defaultValue.GetType() == typeof(bool)) {
					// This is to ensure it formats properly in >> config list
					defValueStr = defValueStr.ToLower();
				}
				SetConfigurationValue(configKey, defValueStr);
				value = defaultValue;
				throw new MalformedConfigDataException($"WARNING: Config key `{configKey}` attempted to read the value from the configuration file, but it failed! Reason: Could not cast `{v}` into type {typeof(T).Name}. It has been set to its default value of {defaultValue.ToString()}");
			}
			value = retn;
		}

		public delegate void ConfigValueChanged(BotContext context, string key, string oldValue, string newValue, bool valueJustCreated);
		//public static event ConfigValueChanged OnAnyConfigValueChanged;
		public event ConfigValueChanged OnConfigValueChanged;

		/// <summary>
		/// Set a configuration value in the config file.
		/// </summary>
		/// <param name="key">The key of the config option</param>
		/// <param name="value">The value of the config option</param>
		/// <param name="dontSaveOnWrite">If this is true, the configuration will not automatically save after setting this value. The user must call it manually. This is used to prevent repeated opening and closing of the file.</param>
		public void SetConfigurationValue(string key, string value, bool dontSaveOnWrite = false) {
			string oldValue = GetConfigurationValue(key);
			ConfigValues[key] = value;
			if (!dontSaveOnWrite) {
				SaveConfigurationFile();
			} else {
				HasUnsavedChanges = true;
			}

			if (BaseHandler != null && BaseHandler.Context != null) {
				try { OnConfigValueChanged(BaseHandler.Context, key, oldValue, value, oldValue == null && value != null); } catch (Exception) { }
			}
			else {
				try { OnConfigValueChanged(null, key, oldValue, value, (oldValue == null && value != null)); } catch (Exception) { }
			}
			//OnAnyConfigValueChanged(null, key, oldValue, value, false);
		}

		/// <summary>
		/// Internal method to set the config value from a default. Contains args to fire the event.
		/// </summary>
		/// <param name="key">The key of the config value</param>
		/// <param name="oldValue">The value before being changed</param>
		/// <param name="newValue">The value after being changed</param>
		/// <param name="wasJustCreated">Whether or not the value is now registered in config and wasn't regsitered before this.</param>
		private void SetConfigurationValue(string key, string oldValue, string newValue, bool wasJustCreated) {
			ConfigValues[key] = newValue;
			SaveConfigurationFile();

			if (BaseHandler != null && BaseHandler.Context != null) {
				try { OnConfigValueChanged(BaseHandler.Context, key, oldValue, newValue, wasJustCreated); } catch (Exception) { }
			}
			else {
				try { OnConfigValueChanged(null, key, oldValue, newValue, wasJustCreated); } catch (Exception) { }
			}
			//OnAnyConfigValueChanged(null, key, oldValue, newValue, wasJustCreated);
		}

		/// <summary>
		/// Returns a configuration value from the config file.
		/// </summary>
		/// <param name="key">The key of the config value</param>
		/// <param name="defaultValue">The default value to get if it doesn't exist</param>
		/// <param name="writeIfDoesntExist">Write the default value if it doesn't exist.</param>
		/// <returns></returns>
		public string GetConfigurationValue(string key, string defaultValue = null, bool writeIfDoesntExist = false, bool reloadConfigFile = false) {
			if (reloadConfigFile) {
				ReloadFromConfigFile();
			}
			bool valueExists = ConfigValues.TryGetValue(key, out string value);
			if (!valueExists) {
				if (writeIfDoesntExist) {
					SetConfigurationValue(key, value, defaultValue, true);
				}
				return defaultValue;
			}
			return value;
		}

		/// <summary>
		/// Removes a config value from the config array. Returns whether or not the key existed and was removed.
		/// </summary>
		/// <param name="key">The key to remove</param>
		public bool RemoveConfigurationValue(string key) {
			if (ConfigValues.ContainsKey(key)) {
				ConfigValues.Remove(key);
				SaveConfigurationFile();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Generates a list of all config values.
		/// </summary>
		/// <returns></returns>
		public string ListConfigurationValues() {
			string list = "```\n";
			foreach (string key in ConfigValues.Keys) {
				list += "§aConfigArray[§3" + key + "§a]=§b" + ConfigValues[key] + "§a\n";
			}
			list += "```";
			return list;
		}

		/// <summary>
		/// Generates a list of all configuration keys.
		/// </summary>
		/// <returns></returns>
		public string[] GetConfigurationKeys() {
			return ConfigValues.Keys.ToArray();
		}

		/// <summary>
		/// Returns whether or not a configuration value exists.
		/// </summary>
		/// <param name="key">The key to search for</param>
		/// <returns></returns>
		public bool ConfigurationValueExists(string key) {
			return ConfigValues.ContainsKey(key);
		}

		/// <summary>
		/// Saves the config file. This should only be used if <see cref="SetConfigurationValue(string, string, bool)"/> was called with its dontSaveOnWrite parameter set to true at some point before you feel the need to call this, otherwise, it will have been automatically called by the set operation.
		/// </summary>
		public void SaveConfigurationFile() {
			StreamWriter dataStream = BaseHandler.CreateText(ConfigFileName);
			foreach (string key in ConfigValues.Keys) {
				string value = ConfigValues[key];
				dataStream.WriteLine(key + " " + value);
			}
			dataStream.Flush();
			dataStream.Close();
			HasUnsavedChanges = false;
		}

		/// <summary>
		/// Loads the config file and populates the array.
		/// </summary>
		private void LoadConfigurationFile() {
			if (HasUnsavedChanges) {
				XanBotLogger.WriteLine("§4A request to reload the config file was made, but there were unsaved manual changes in the config file made via code. The config file will not be reloaded, which may have adverse effects!", true);
				return;
			}
			string[] lines = BaseHandler.GetLinesOfFile(ConfigFileName);
			foreach (string line in lines) {
				string[] values = line.Split(' ');
				string index = values[0];
				string stringValue = line.Substring(index.Length + 1).Replace("\n", "").Replace("\r", "");
				ConfigValues[index] = stringValue;
			}
		}

		/// <summary>
		/// Reloads the stored config.
		/// </summary>
		public void ReloadFromConfigFile() {
			if (HasUnsavedChanges) {
				XanBotLogger.WriteLine("§4A request to reload the config file was made, but there were unsaved manual changes in the config file made via code. The config file will not be reloaded, which may have adverse effects!", true);
				return;
			}
			ConfigValues.Clear();
			LoadConfigurationFile();
		}

		/// <summary>
		/// Creates a new <see cref="XConfiguration"/> (or gets an existing one) from an underlying <see cref="XFileHandler"/>. This caches the object.
		/// </summary>
		/// <param name="handler">The underlying <see cref="XFileHandler"/> that this <see cref="XConfiguration"/> should extend.</param>
		/// <returns></returns>
		public static XConfiguration GetConfigurationUtility(XFileHandler handler, string cfgFileName = "configuration.cfg") {
			if (ConfigurationTies.ContainsKey(handler) && ConfigurationTies[handler].ContainsKey(cfgFileName)) {
				return ConfigurationTies[handler][cfgFileName];
			}

			if (!ConfigurationTies.ContainsKey(handler))
				ConfigurationTies[handler] = new Dictionary<string, XConfiguration>();
			

			XConfiguration cfg = new XConfiguration(handler, cfgFileName);
			ConfigurationTies[handler][cfgFileName] = cfg;
			return cfg;
		}

		/// <summary>
		/// Creates a new <see cref="XConfiguration"/> (or gets an existing one) from the specified <see cref="BotContext"/> by first creating or getting the <see cref="XFileHandler"/> for said context. This caches the object.
		/// </summary>
		/// <param name="context">The <see cref="BotContext"/> that this <see cref="XConfiguration"/> should target.</param>
		/// <returns></returns>
		public static XConfiguration GetConfigurationUtility(BotContext context, string cfgFileName = "configuration.cfg") {
			return GetConfigurationUtility(XFileHandler.GetFileHandlerForBotContext(context), cfgFileName);
		}
	}
}
