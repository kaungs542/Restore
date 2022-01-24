using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZendeskApi_v2;
using ZendeskApi_v2.Models.Requests;
using ZendeskApi_v2.Models.Tickets;
using ZendeskApi_v2.Models.Users;
using ZendeskApi_v2.Requests;

namespace API.Helpers
{
    public class ZendeskHelper
    {

        private readonly IZendeskApi _api;
        private readonly int _pageSize;
        string userName = "kaungs542@gmail.com"; // the user that will be logging in the API aka the call center staff
        string userPassword = "K@ungs@n1993";
        string companySubDomain = "prudential1855"; // sub-domain for the account with Zendesk
        public ZendeskHelper(int pageSize = 100)
        {

            _api = new ZendeskApi_v2.ZendeskApi(companySubDomain, userName, userPassword);
            _pageSize = pageSize;
        }


        public IZendeskApi GetZendeskApi()
        {
            return _api;
        }


        public async Task<List<Ticket>> GetTickets(string userEmailToSearchFor)
        {
            var userResponse = await _api.Search.SearchForAsync<User>(userEmailToSearchFor);

            var userId = userResponse.Results[0].Id.Value;
            var tickets = new List<Ticket>();

            var ticketResponse = await _api.Tickets.GetTicketsByUserIDAsync(userId, _pageSize, sideLoadOptions: TicketSideLoadOptionsEnum.Users | TicketSideLoadOptionsEnum.Comment_Count); // default per page is 100

            do
            {
                tickets.AddRange(ticketResponse.Tickets);

                if (!string.IsNullOrWhiteSpace(ticketResponse.NextPage))
                {
                    ticketResponse = await _api.Tickets.GetByPageUrlAsync<GroupTicketResponse>(ticketResponse.NextPage, _pageSize);
                }


            } while (tickets.Count != ticketResponse.Count);
            return tickets;
        }

        public async Task<List<Comment>> GetComments(Ticket ticket)
        {
            var comments = new List<Comment>();

            if (ticket.CommentCount > 0)
            {
                var commnetResponse = await _api.Tickets.GetTicketCommentsAsync(ticket.Id.Value, _pageSize);

                do
                {
                    comments.AddRange(commnetResponse.Comments);

                    if (!string.IsNullOrWhiteSpace(commnetResponse.NextPage))
                    {
                        commnetResponse = await _api.Tickets.GetByPageUrlAsync<GroupCommentResponse>(commnetResponse.NextPage, _pageSize);
                    }
                } while (comments.Count != commnetResponse.Count);
            }

            return comments;
        }
    }

}