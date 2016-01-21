/* Options:
Date: 2016-01-21 03:41:54
Version: 4.00
BaseUrl: http://chat.servicestack.net

//GlobalNamespace: 
//MakePartial: True
//MakeVirtual: True
//MakeDataContractsExtensible: False
//AddReturnMarker: True
//AddDescriptionAsComments: True
//AddDataContractAttributes: False
//AddIndexesToDataMembers: False
//AddGeneratedCodeAttributes: False
//AddResponseStatus: False
//AddImplicitVersion: 
//InitializeCollections: True
//IncludeTypes: 
//ExcludeTypes: 
//AddDefaultXmlNamespace: http://schemas.servicestack.net/types
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using ServiceStack.DataAnnotations;
using Chat;


namespace Chat
{

    public partial class ChatMessage
    {
        public virtual long Id { get; set; }
        public virtual string Channel { get; set; }
        public virtual string FromUserId { get; set; }
        public virtual string FromName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Message { get; set; }
        public virtual string UserAuthId { get; set; }
        public virtual bool Private { get; set; }
    }

    [Route("/chathistory")]
    public partial class GetChatHistory
        : IReturn<GetChatHistoryResponse>
    {
        public GetChatHistory()
        {
            Channels = new string[]{};
        }

        public virtual string[] Channels { get; set; }
        public virtual long? AfterId { get; set; }
        public virtual int? Take { get; set; }
    }

    public partial class GetChatHistoryResponse
    {
        public GetChatHistoryResponse()
        {
            Results = new List<ChatMessage>{};
        }

        public virtual List<ChatMessage> Results { get; set; }
        public virtual ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/channels/{Channel}/chat")]
    public partial class PostChatToChannel
        : IReturn<ChatMessage>
    {
        public virtual string From { get; set; }
        public virtual string ToUserId { get; set; }
        public virtual string Channel { get; set; }
        public virtual string Message { get; set; }
        public virtual string Selector { get; set; }
    }

    [Route("/channels/{Channel}/raw")]
    public partial class PostRawToChannel
        : IReturnVoid
    {
        public virtual string From { get; set; }
        public virtual string ToUserId { get; set; }
        public virtual string Channel { get; set; }
        public virtual string Message { get; set; }
        public virtual string Selector { get; set; }
    }
}

