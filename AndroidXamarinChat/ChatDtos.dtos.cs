/* Options:
Date: 2016-02-04 22:01:48
Version: 4.00
Tip: To override a DTO option, remove "//" prefix before updating
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

    [Route("/account")]
    public partial class GetUserDetails
        : IReturn<GetUserDetailsResponse>
    {
    }

    public partial class GetUserDetailsResponse
    {
        public virtual string Provider { get; set; }
        public virtual string UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string FullName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Company { get; set; }
        public virtual string Email { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual DateTime? BirthDate { get; set; }
        public virtual string BirthDateRaw { get; set; }
        public virtual string Address { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Country { get; set; }
        public virtual string Culture { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Language { get; set; }
        public virtual string MailAddress { get; set; }
        public virtual string Nickname { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string TimeZone { get; set; }
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

