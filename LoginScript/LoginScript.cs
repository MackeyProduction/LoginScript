using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using w3bot.Api;
using w3bot.Input;
using w3bot.Script;
using w3bot.Util;

namespace LoginScript
{
	[ScriptManifest("Login Script", "Login", "Login test.", "NoChoice", 1.0)]
	public class LoginScript : AbstractScript
	{
		private Frame _frame;
		private Point _loginButton;
		private string _loginResponse;
		private int _tries = 0;
		private int _maxTries = 3;
		private string _response;

		public enum State
		{
			Login,
			LoginSuccessful,
			LoginFailed,
			Pending,
		}

		public string FetchLoginResponse()
		{
			Task.Run(() =>
			{
				var jsData = Browser.ExecuteJavascript("(function() { var result=document.getElementsByTagName('body')[0].innerText; return result; })()");

				if (jsData.Result != null)
				{
					var jsResponse = (JavascriptResponse)jsData.Result;

					_response = (string)jsResponse.Result;
				}
			});

			return _response;
		}

		public State GetState()
		{
			_loginButton = _frame.FindPixel(0, 123, 255, 3);
			if (_loginButton != Point.Empty)
			{
				return State.Login;
			}

			_loginResponse = FetchLoginResponse();
			switch (_loginResponse)
			{
				case "Logged in successfully!":
					return State.LoginSuccessful;
				case "Login failed. Please check your user credentials.":
					return State.LoginFailed;
				default:
					break;
			}

			return State.Pending;
		}

		public override void OnStart()
		{
			Status.Log("Test script has been started.");

			CreateBrowserWindow(); // create a new browser window

			_frame = Methods.Frame; // initialize the frame object

			Browser.Navigate("https://tutorials.w3bot.org/login/");
		}

		public override int OnUpdate()
		{
			// check if browser is ready
			if (Browser.IsReady)
			{
				if (_tries == _maxTries)
				{
					Status.Log("Maximum of tries reached. Stopping script...");
					return -1;
				}

				if (GetState() == State.Login)
				{
					var usernameTextbox = "document.getElementById('username').value = 'user';";
					var passwordTextbox = "document.getElementById('password').value = 'mysecretpassword';";

					// execute javascript
					Browser.ExecuteJavascript(usernameTextbox);
					Browser.ExecuteJavascript(passwordTextbox);

					Mouse.LeftClick(_loginButton);
					Sleep(2000);
				}

				if (GetState() == State.LoginSuccessful)
				{
					Status.Log("Login successful. Stopping script...");
					return -1;
				}

				if (GetState() == State.LoginFailed)
				{
					Status.Log(String.Format("Login failed. Try again... ({0}/{1})", _tries, _maxTries));
					_tries++;

					// navigating back to login page
					Browser.Navigate("https://tutorials.w3bot.org/login/");
				}

				if (GetState() == State.Pending)
				{
					Status.Log("Pending...");
				}
			}

			return 1000;
		}

		public override void OnFinish()
		{
			Status.Log("Thank you for using my script.");
		}
	}
}