﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Domains.Security.Core;
using NWheels.Entities;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Domains.Security
{
    public class ChangePasswordTransactionScript : ITransactionScript
    {
        private readonly IFramework _framework;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChangePasswordTransactionScript(IFramework framework, IAuthenticationProvider authenticationProvider, ISessionManager sessionManager)
        {
            _framework = framework;
            _authenticationProvider = authenticationProvider;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SecurityCheck.AllowAnonymous]
        public virtual void Execute(
            string loginName,
            string oldPassword,
            string newPassword, 
            bool passwordExpired)
        {
            using (_sessionManager.JoinGlobalSystem())
            {
                IApplicationDataRepository authenticationContext;
                IQueryable<IUserAccountEntity> userAccountQuery;
                OpenAuthenticationContext(out authenticationContext, out userAccountQuery);

                using (authenticationContext)
                {
                    IUserAccountEntity userAccount = null;

                    try
                    {
                        if (!passwordExpired)
                        {
                            _authenticationProvider.Authenticate(
                                userAccountQuery, loginName, SecureStringUtility.ClearToSecure(oldPassword), out userAccount);
                        }
                        else
                        {
                            _authenticationProvider.AuthenticateByExpiredPassword(
                                userAccountQuery, loginName, SecureStringUtility.ClearToSecure(oldPassword), out userAccount);
                        }
                    }
                    catch (DomainFaultException<LoginFault> error)
                    {
                        if (error.FaultCode != LoginFault.PasswordExpired)
                        {
                            throw;
                        }
                    }

                    userAccount.As<UserAccountEntity>().SetPassword(SecureStringUtility.ClearToSecure(newPassword));
                    UpdateUserAccount(authenticationContext, userAccount);

                    authenticationContext.CommitChanges();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OpenAuthenticationContext(out IApplicationDataRepository context, out IQueryable<IUserAccountEntity> userAccounts)
        {
            var userAccountsContext = _framework.NewUnitOfWork<IUserAccountDataRepository>();

            context = userAccountsContext;
            userAccounts = userAccountsContext.AllUsers.AsQueryable();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void UpdateUserAccount(IApplicationDataRepository context, IUserAccountEntity userAccount)
        {
            ((IUserAccountDataRepository)context).AllUsers.Update(userAccount);
        }
    }
}