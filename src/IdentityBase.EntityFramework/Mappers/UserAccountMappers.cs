namespace IdentityBase.EntityFramework.Mappers
{
    using AutoMapper;
    using IdentityBase.EntityFramework.Entities;

    public static class UserAccountMappers
    {
        static UserAccountMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
                cfg.AddProfile<UserAccountProfile>())
                    .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        public static Models.UserAccount ToModel(this UserAccount entity)
        {
            return entity == null ? null : Mapper
                .Map<Models.UserAccount>(entity);
        }

        public static UserAccount ToEntity(this Models.UserAccount model)
        {
            return model == null ? null : Mapper
                .Map<UserAccount>(model);
        }

        public static void UpdateEntity(
            this Models.UserAccount model, UserAccount entity)
        {
            Mapper.Map(model, entity);
        }

        public static Models.ExternalAccount ToModel(
            this ExternalAccount entity)
        {
            return entity == null ? null : Mapper
                .Map<Models.ExternalAccount>(entity);
        }

        public static ExternalAccount ToEntity(
            this Models.ExternalAccount model)
        {
            return model == null ? null : Mapper
                .Map<ExternalAccount>(model);
        }

        public static void UpdateEntity(
            this Models.ExternalAccount model, ExternalAccount entity)
        {
            Mapper.Map(model, entity);
        }
    }
}