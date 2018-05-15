using System;
using System.Web.Security;
using System.Web.UI.WebControls;
using Sitecore.Controls;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Security.Accounts;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;

namespace Sitecore.Support.Shell.Applications.Security.SetPassword
{
  /// <summary>
  /// Represents a GridDesignerPage.
  /// </summary>
  public class SetPasswordPage : DialogPage
  {
    #region Fields

    /// <summary></summary>
    protected TextBox OldPassword;
    /// <summary></summary>
    protected TextBox NewPassword;
    /// <summary></summary>
    protected TextBox ConfirmPassword;
    /// <summary></summary>
    protected System.Web.UI.WebControls.Label UserName;
    /// <summary></summary>
    protected System.Web.UI.WebControls.Label DomainName;
    /// <summary></summary>
    protected Edit RandomPassword;
    /// <summary></summary>
    protected ThemedImage Portrait;

    #endregion

    public const string FAILED_TO_SET_THE_PASSWORD_POSSIBLE_REASONS_ARE_1_THE_OLD_PASSWORD_IS_INCORRECT_2_THE_NEW_PASSWORD_DOES_NOT_MEET_THE_REQUIREMENTS = "Failed to set the password.\n\nPossible reasons are:\n\n1) The old password is incorrect.\n2) The new password does not meet the security requirements.\n\nTo learn about the password security requirements, please consult your administrator.";

    #region Fields

    /// <summary>
    /// Gets or sets a value indicating whether this instance has generated password.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance has generated password; otherwise, <c>false</c>.
    /// </value>
    public bool HasGeneratedPassword
    {
      get
      {
        object state = ViewState["HasGeneratedPassword"];
        if (state == null)
        {
          return false;
        }

        return (bool)state;
      }
      set
      {
        ViewState["HasGeneratedPassword"] = value;
      }
    }

    #endregion

    #region Protected methods

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");

      base.OnLoad(e);

      User user = User.FromName(WebUtil.GetSafeQueryString("us"), true);
      Assert.IsNotNull(user, typeof(User));

      string portrait = ImageBuilder.ResizeImageSrc(user.Profile.Portrait, 48, 48).Trim();
      Assert.IsNotNull(portrait, "portrait");

      if (portrait != string.Empty)
        this.Portrait.Src = portrait;

      UserName.Text = user.GetLocalName();
      DomainName.Text = user.GetDomainName();

      RandomPassword.Value = Translate.TextByLanguage(Texts.NoPasswordHasBeenGeneratedYet, Language.Current);
    }

    /// <summary>Handles a click on the OK button.</summary>
    /// <remarks>When the user clicks OK, the dialog is closed by calling
    /// the <see cref="Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.</remarks>
    protected override void OK_Click()
    {
      string oldPassword = OldPassword.Text;
      string newPassword = NewPassword.Text;
      string confirmPassword = ConfirmPassword.Text;

      if (!string.IsNullOrEmpty(oldPassword) || !string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmPassword) || !HasGeneratedPassword)
      {
        if (string.IsNullOrEmpty(oldPassword))
        {
          SheerResponse.Alert(Translate.Text(Texts.YOU_MUST_SUPPLY_THE_OLD_PASSWORD_IF_YOU_DO_NOT_KNOW_THE_OLD_PASSWORD_GENERATE_A_NEW_PASSWORDBY_CLICKING_THE_GENERATE_BUTTON_BELOW));
          return;
        }

        if (string.IsNullOrEmpty(newPassword))
        {
          SheerResponse.Alert(Translate.Text(Texts.YOU_MUST_SUPPLY_A_NEW_PASSWORD));
          return;
        }

        if (newPassword != confirmPassword)
        {
          SheerResponse.Alert(Translate.Text(Texts.THE_PASSWORDS_DO_NOT_MATCH));
          return;
        }

        MembershipUser user = GetUser();
        Assert.IsNotNull(user, typeof(User));

        var success = false;

        try
        {
          success = user.ChangePassword(oldPassword, newPassword);
        }
        catch (ArgumentException)
        {
        }

        if (!success)
        {
          SheerResponse.Alert(Translate.Text(FAILED_TO_SET_THE_PASSWORD_POSSIBLE_REASONS_ARE_1_THE_OLD_PASSWORD_IS_INCORRECT_2_THE_NEW_PASSWORD_DOES_NOT_MEET_THE_REQUIREMENTS));
          return;
        }

        SheerResponse.Alert(Translate.Text(Texts.THE_PASSWORD_HAS_BEEN_CHANGED));
      }

      base.OK_Click();
    }

    /// <summary>
    /// Handles the Generate_ click event.
    /// </summary>
    protected void Generate_Click()
    {
      ClientPipelineArgs args = ContinuationManager.Current.CurrentArgs as ClientPipelineArgs;
      Assert.IsNotNull(args, typeof(ClientPipelineArgs));

      MembershipUser user = GetUser();
      Assert.IsNotNull(user, typeof(User));

      if (args.IsPostBack)
      {
        if (args.Result == "yes")
        {
          string password = null;

          try
          {
            password = user.ResetPassword();
          }
          catch (NotSupportedException e)
          {
            SheerResponse.Alert(e.Message);
            return;
          }

          SheerResponse.SetStyle("RandomPassword", "color", "Black");
          SheerResponse.Eval("document.getElementById('RandomPassword').disabled = false;");
          SheerResponse.SetAttribute("RandomPassword", "readonly", "readonly");

          RandomPassword.Value = password;

          HasGeneratedPassword = true;

          SheerResponse.Alert(Translate.Text(Texts.THE_PASSWORD_HAS_BEEN_CHANGED));
        }
      }
      else
      {
        SheerResponse.Confirm(Translate.Text(Texts.ARE_YOU_SURE_YOU_WANT_TO_RESET_THE_PASSWORD));
        args.WaitForPostBack();
      }
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <returns>The user.</returns>
    static MembershipUser GetUser()
    {
      string userName = WebUtil.GetQueryString("us");

      return Membership.GetUser(userName);
    }

    #endregion
  }
}