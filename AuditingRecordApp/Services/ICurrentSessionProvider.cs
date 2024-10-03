﻿using System.Security.Claims;

namespace AuditingRecordApp.Services;

public interface ICurrentSessionProvider
{
    Guid? GetUserId();
}

internal sealed class CurrentSessionProvider : ICurrentSessionProvider
{
    private readonly Guid? _currentUserId;

    public CurrentSessionProvider(IHttpContextAccessor accessor)
    {
        var userId = accessor.HttpContext?.User.FindFirstValue("userid");
        if (userId is null)
        {
            return;
        }

        _currentUserId = Guid.TryParse(userId, out var guid) ? guid : null;
    }

    public Guid? GetUserId() => _currentUserId;
}