using Swashbuckle.AspNetCore.Annotations;

namespace CodeEventsAPI.Controllers {
  public struct SwaggerConfig {
    public const string PublicTag = "Public";
    public const string RequireOwnerTag = "Protected, Authorization: Owner";
    public const string RequireMemberTag = "Protected, Authorization: Member";
    public const string RequireUserTag = "Protected, Authorization: None";
  }
}
