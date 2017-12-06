namespace IdentityBase.IntegrationTests
{
    public static class Schemas
    {
        public static string InvitationsPostResponse = @"{
                'type': 'object',
                'additionalProperties' : false,
                'properties': {
                'id': {
                    'type': 'string'
                },
                'email': {
                    'type': [
                    'string',
                    'null'
                    ]
                },
                'createdAt': {
                    'type': 'string',
                    'format': 'date-time'
                },
                'verificationKeySentAt': {
                    'type': 'string',
                    'format': 'date-time'      
                }
                },
                'required': [
                    'id',
                    'email',
                    'createdAt',
                    'verificationKeySentAt'
                ]
            }";

        public static string ErrorResponse = @"{
                'type': 'object',
                'additionalProperties' : false,
                'properties': {
                'type': {
                    'type': 'string'
                },
                'error': {
                    'type': 'object'
                }
            }";
    }
}
