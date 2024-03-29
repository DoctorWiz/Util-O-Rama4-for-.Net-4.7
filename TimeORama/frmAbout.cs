﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using xUtilities;
using LORUtils;

namespace VampORama
{
	partial class frmAbout : Form
	{

		string applicationName = "Vamp-O-Rama";
		bool xLightsInstalled = false;
		bool lightORamaInstalled = false;

		public frmAbout()
		{
			InitializeComponent();

			//applicationName = AssemblyTitle;
			string fooo = xUtils.ShowDirectory;
			if (fooo.Length > 3) xLightsInstalled = true;
			fooo = utils.DefaultSequencesPath;
			if (fooo.Length > 3) lightORamaInstalled = true;
			if (xLightsInstalled && !lightORamaInstalled) // xLights Only
			{
				applicationName = "Vamperizer";
				labelUtils.Text = "xUtils";


				lblCommunity.Text = "of the                         community.";
				picxLights.Visible = true;
				picxLights.Left = 64;
				picLOR.Visible = false;
				lblDisclaimer.Text = "The xUtils suite and this application are not a product of, nor officially endorsed in any way by" +
					" the xLights project.  This is purely the work of Dr. Wizard and W⚡zster.  Please do not contact the xLights team for" +
					" support regarding the xUtils applications.";
				lblEmail.Text = "dev.xutils@wizster.com";
			}
			if (lightORamaInstalled && !xLightsInstalled) // Light-O-Rama Only
			{
				lblCommunity.Text = "of the                         community.";
				picxLights.Visible = false;
				picLOR.Left = 64;
				picLOR.Visible = true;
				lblDisclaimer.Text = "The Util-O-Rama suite and this application are not a product of, nor officially endorsed in any way by" +
					" the Light-O-Rama company.  This is purely the work of Dr. Wizard and W⚡zster.  Please do not contact the Light-O-Rama for" +
					" support regarding the Util-O-Rama applications.";
				lblEmail.Text = "dev.utilorama@wizster.com";
			}
			if (lightORamaInstalled && !xLightsInstalled) // Both Light-O-Rama and xLights
			{
				lblCommunity.Text = "of the                         and                         community.";
				picxLights.Visible = true;
				picLOR.Left = 64;
				picxLights.Left = 184;
				picLOR.Visible = true;
				lblDisclaimer.Text = "The Util-O-Rama and xUtils suites and this application are not a product of, nor officially endorsed in any way by" +
					" the Light-O-Rama company or the xLights team.  This is purely the work of Dr. Wizard and W⚡zster.  Please do not contact " +
					"Light-O-Rama or the xLights team for" +
					" support regarding the Util-O-Rama or xUtils applications.";
				lblEmail.Text = "dev.utilorama@wizster.com";
			}



			this.Text = String.Format("About {0}", applicationName);
			this.labelProductName.Text = applicationName;
			string ver = String.Format("Version {0}", AssemblyVersion);
			string[] vparts = AssemblyVersion.Split('.');
			if (Int16.Parse(vparts[1]) < 51)
			{
				ver = "Beta " + ver + " β";

			}

			this.labelVersion.Text = ver;
			//this.labelCopyright.Text = AssemblyCopyright;
			//this.labelCompanyName.Text = AssemblyCompany;
			this.textBoxDescription.Text = AssemblyDescription;

			labelSuite.Text = applicationName + labelSuite.Text;
			labelFreeware.Text = applicationName + labelFreeware.Text;



		}

		#region Assembly Attribute Accessors

		public string AssemblyTitle
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}
		#endregion

		private void frmAbout_Load(object sender, EventArgs e)
		{
			//labelAuthorName.Links.Add("http://drwiz.net");
			//labelCompanyName.Links.Add("http://wizster.com");
			//labelAuthorName.Links.Add(6, 4, "http://drwiz.net");
			//labelCompanyName.Links.Add(6, 4, "http://wizster.com");
		}

		public void InitForm()
		{
			//logoPictureBox.Image = this.Icon.ToBitmap();
		}

		private void frmAbout_Shown(object sender, EventArgs e)
		{
			//logoPictureBox.Image = this.Icon.ToBitmap();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			this.Hide();
			this.Close();
		}

		private void labelAuthorName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://drwiz.net/");
		}

		private void labelCompanyName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://wizster.com/");
		}

		private void labelLOR_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www1.lightorama.com/");
		}

		private void labelGPL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://www.gnu.org/licenses/gpl-3.0.en.html");
		}

		private void labelBugs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
		}

		private void label4_Click(object sender, EventArgs e)
		{

		}

		private void okButton_Click_1(object sender, EventArgs e)
		{
			this.Hide();
			this.Close();
		}

		private void picGPL_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("https://www.gnu.org/licenses/gpl-3.0.en.html");
		}

		private void picLOR_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www1.lightorama.com/");
		}

		private void labelUtils_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (xLightsInstalled && !lightORamaInstalled)
			{
				System.Diagnostics.Process.Start("http://wizlights.com/xutils/");
			}
			else
			{
				System.Diagnostics.Process.Start("http://wizlights.com/utilorama/");
			}
		}

		private void picLOR_MouseEnter(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Hand;
		}

		private void picLOR_MouseLeave(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Default;
		}

		private void picGPL_MouseEnter(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Hand;
		}

		private void picGPL_MouseLeave(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Default;
		}

		private void picIcon_MouseEnter(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Hand;
		}

		private void picIcon_MouseLeave(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Default;
		}

		private void picIcon_Click(object sender, EventArgs e)
		{
			if (xLightsInstalled && !lightORamaInstalled)
			{
				System.Diagnostics.Process.Start("http://wizlights.com/xutils/" + applicationName.ToLower() + "/");
			}
			else
			{
				System.Diagnostics.Process.Start("http://wizlights.com/utilorama/" + applicationName.ToLower() + "/");
			}
		}

		private void label5_Click(object sender, EventArgs e)
		{

		}

		private void lblEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (xLightsInstalled && !lightORamaInstalled)
			{
				System.Diagnostics.Process.Start("mailto:dev.xutils@wizster.com");
			}
			else
			{
				System.Diagnostics.Process.Start("mailto:dev.utilorama@wizster.com");
			}
		}

		private void picxLights_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://xlights.org/");

		}
	}
}
