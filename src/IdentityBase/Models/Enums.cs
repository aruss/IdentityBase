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
        SelfService,
        Invitation
    }
}
