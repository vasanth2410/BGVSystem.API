using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
