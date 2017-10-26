namespace IdentityBase.Models
{
    public enum VerificationKeyPurpose
    {
        ResetPassword,
        ChangeEmail,
        ChangeMobile,
        ConfirmAccount,
        MergeAccount
    }

    public enum CreationKind
    {
        /// <summary>
        /// Created via frontend by user it self.
        /// </summary>
        SelfService,

        /// <summary>
        /// User was invited via another user. 
        /// </summary>
        Invitation
    }
}
