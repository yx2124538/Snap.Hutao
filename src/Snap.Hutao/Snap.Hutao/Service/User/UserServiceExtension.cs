// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.Database;
using Snap.Hutao.ViewModel.User;
using Snap.Hutao.Web.Hoyolab.Takumi.Binding;
using BindingUser = Snap.Hutao.ViewModel.User.User;
using EntityUser = Snap.Hutao.Model.Entity.User;

namespace Snap.Hutao.Service.User;

// For performance reason, extension method should avoid using LINQ
internal static class UserServiceExtension
{
    public static ValueTask<bool> RefreshCookieTokenAsync(this IUserService userService, BindingUser user)
    {
        return userService.RefreshCookieTokenAsync(user.Entity);
    }

    public static async ValueTask<UserGameRole?> GetUserGameRoleByUidAsync(this IUserService userService, string uid)
    {
        AdvancedDbCollectionView<BindingUser, EntityUser> users = await userService.GetUsersAsync().ConfigureAwait(false);
        foreach (BindingUser bindingUser in users.Source)
        {
            foreach (UserGameRole role in bindingUser.UserGameRoles.Source)
            {
                if (role.GameUid == uid)
                {
                    return role;
                }
            }
        }

        return default;
    }

    public static async ValueTask<BindingUser?> GetCurrentUserAsync(this IUserService userService)
    {
        AdvancedDbCollectionView<BindingUser, EntityUser> users = await userService.GetUsersAsync().ConfigureAwait(false);
        return users.CurrentItem;
    }

    public static async ValueTask<UserGameRole?> GetCurrentUserGameRoleAsync(this IUserService userService)
    {
        AdvancedDbCollectionView<BindingUser, EntityUser> users = await userService.GetUsersAsync().ConfigureAwait(false);
        return users.CurrentItem?.UserGameRoles.CurrentItem;
    }

    public static async ValueTask<string?> GetCurrentUidAsync(this IUserService userService)
    {
        AdvancedDbCollectionView<BindingUser, EntityUser> users = await userService.GetUsersAsync().ConfigureAwait(false);
        return users.CurrentItem?.UserGameRoles?.CurrentItem?.GameUid;
    }

    public static async ValueTask<UserAndUid?> GetCurrentUserAndUidAsync(this IUserService userService)
    {
        AdvancedDbCollectionView<BindingUser, EntityUser> users = await userService.GetUsersAsync().ConfigureAwait(false);
        UserAndUid.TryFromUser(users.CurrentItem, out UserAndUid? userAndUid);
        return userAndUid;
    }

    public static async ValueTask<bool> SetCurrentUserByUidAsync(this IUserService userService, string uid)
    {
        AdvancedDbCollectionView<BindingUser, EntityUser> users = await userService.GetUsersAsync().ConfigureAwait(false);
        BindingUser? user = users.Source.SingleOrDefault(u => u.UserGameRoles.Source.Any(r => r.GameUid == uid));

        if (user is null)
        {
            return false;
        }

        await userService.TaskContext.SwitchToMainThreadAsync();
        users.MoveCurrentTo(user);

        return true;
    }

    public static async ValueTask<BindingUser?> GetUserByMidAsync(this IUserService userService, string mid)
    {
        AdvancedDbCollectionView<BindingUser, EntityUser> users = await userService.GetUsersAsync().ConfigureAwait(false);
        foreach (BindingUser user in users.Source)
        {
            if (user.Entity.Mid == mid)
            {
                return user;
            }
        }

        return default;
    }
}