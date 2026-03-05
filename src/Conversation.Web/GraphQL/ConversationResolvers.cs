using ConversationModel = Conversation.Web.Models.Conversation;
using ProjectModel = Conversation.Web.Models.Project;
using HotChocolate.Types;

namespace Conversation.Web.GraphQL;

[ExtendObjectType<ConversationModel>]
public sealed class ConversationResolvers
{
    public ProjectModel Project([Parent] ConversationModel conversation) =>
        new()
        {
            Id = conversation.ProjectId
        };
}
