// <copyright file="AuthorizationPolicies.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Authorization
{
    /// <summary>
    ///     Defines authorization policies for the Hotshot Logistics application.
    /// </summary>
    public static class AuthorizationPolicies
    {
        /// <summary>
        ///     Policy name for Admin role.
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        ///     Policy name for Manager role.
        /// </summary>
        public const string Manager = "Manager";

        /// <summary>
        ///     Policy name for Driver role.
        /// </summary>
        public const string Driver = "Driver";

        /// <summary>
        ///     Policy name for Customer role.
        /// </summary>
        public const string Customer = "Customer";

        /// <summary>
        ///     Policy name for Admin or Manager roles.
        /// </summary>
        public const string ManagerOrAdmin = "ManagerOrAdmin";

        /// <summary>
        ///     Policy name for Manager or Driver roles.
        /// </summary>
        public const string ManagerOrDriver = "ManagerOrDriver";

        /// <summary>
        ///     Policy name for resource-based access to own resources.
        /// </summary>
        public const string OwnResource = "OwnResource";

        /// <summary>
        ///     Policy name for resource-based access to customer resources.
        /// </summary>
        public const string CustomerResource = "CustomerResource";
    }
}
