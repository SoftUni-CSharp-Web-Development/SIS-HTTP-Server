﻿using System.Linq;
using MishMashWebApp.ViewModels.Channels;
using MishMashWebApp.ViewModels.Home;
using SIS.HTTP.Responses;
using SIS.MvcFramework;

namespace MishMashWebApp.Controllers
{
    public class HomeController : BaseController
    {
        public IHttpResponse Index()
        {
            var user = this.Db.Users.FirstOrDefault(x => x.Username == this.User);
            if (user != null)
            {
                var viewModel = new LoggedInIndexViewModel();
                viewModel.UserRole = user.Role.ToString();

                viewModel.YourChannels = this.Db.Channels.Where(
                        x => x.Followers.Any(f => f.User.Username == this.User))
                    .Select(x => new BaseChannelViewModel
                    {
                        Id = x.Id,
                        Type = x.Type,
                        Name = x.Name,
                        FollowersCount = x.Followers.Count(),
                    }).ToList();

                var followedChannelsTags = this.Db.Channels.Where(
                        x => x.Followers.Any(f => f.User.Username == this.User))
                    .SelectMany(x => x.Tags.Select(t => t.TagId)).ToList();

                viewModel.SuggestedChannels = this.Db.Channels.Where(
                    x => !x.Followers.Any(f => f.User.Username == this.User) &&
                         x.Tags.Any(t => followedChannelsTags.Contains(t.TagId)))
                    .Select(x => new BaseChannelViewModel
                    {
                        Id = x.Id,
                        Type = x.Type,
                        Name = x.Name,
                        FollowersCount = x.Followers.Count(),
                    }).ToList();

                var ids = viewModel.YourChannels.Select(x => x.Id).ToList();
                ids = ids.Concat(viewModel.SuggestedChannels.Select(x => x.Id)).ToList();
                ids = ids.Distinct().ToList();

                viewModel.SeeOtherChannels = this.Db.Channels.Where(x => !ids.Contains(x.Id))
                    .Select(x => new BaseChannelViewModel
                    {
                        Id = x.Id,
                        Type = x.Type,
                        Name = x.Name,
                        FollowersCount = x.Followers.Count(),
                    }).ToList();

                return this.View("Home/LoggedInIndex", viewModel);
            }
            else
            {
                return this.View("Home/Index");
            }
        }
    }
}
