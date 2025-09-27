// <copyright file="ResourceOwnerRequirement.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Authorization requirement for resource ownership.
    /// </summary>
    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the resource type.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOwnerRequirement"/> class.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        public ResourceOwnerRequirement(string resourceType)
        {
            this.ResourceType = resourceType;
        }
    }
}
