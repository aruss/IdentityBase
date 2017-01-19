using Microsoft.EntityFrameworkCore;

namespace ServiceBase.IdentityServer.Public.IntegrationTests
{
    /*
        cdb7b8c3-ccbc-486e-a265-b0b7426fb005
        fafe443f-0829-4be1-9b0f-78dabfcf6c97
        7f741d3e-8a57-4b7e-9f5c-f611d2d67e03
        1e953397-b552-4970-8c8c-c45a1a2a663e
        579d13a5-6c98-4c42-944a-823a2045805e
        e5771c9a-4f19-45e7-be31-740aa825f9f7
        efe37d26-4075-4d7a-8c92-9dba1bbe1d47
        03219d06-9d07-4a6b-8180-807f4101bae1
        2e0025a0-6ba9-498d-b6e6-44140dd6d923
        cfd16155-63ce-4528-8bfc-d427d724f163
        711dec2a-5390-4c4e-b8e9-9a40d704db5f
        da39fc0b-5819-4d1e-aa22-aa6e26c7b46d
        a56fb5cc-7190-4d3b-969d-25ae5c2f9742
        1a41bb4c-f053-4c16-bd35-e12ad5dfca0a
        9fb45c8d-b031-4e41-b82e-dd104fa90fad
        2f3b216d-95f4-4b01-bd9f-a412c4b15d26
     */

    internal static class DbContextExtensions
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}