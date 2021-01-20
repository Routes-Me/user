﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models
{
    public class CommonMessage
    {
        public static string RoleConflict = "Role is associated with other user.";
        public static string BadRequest = "Pass valid data in model.";
        public static string UserRoleRequired = "User role required.";
        public static string UserRoleNotFound = "User role not found.";
        public static string PhoneExist = "Phone number already exist.";   
        public static string EmailExist = "Email already exist.";
        public static string UnableToInsertUserIntoDriver = "Unable to insert user into drivers.";
        public static string UserInsert = "User created successfully.";
        public static string IncorrectUser = "Incorrect username.";
        public static string IncorrectPassword = "Incorrect password.";
        public static string InvalidPasswordFormat = "Incorrect encrypted password string format.";
        public static string IncorrectUserRole = "Incorrect user role.";
        public static string LoginSuccess = "Login successfully.";
        public static string ExceptionMessage = "Something went wrong. Error Message - ";
        public static string PhoneNotExist = "Phone number does not exist in the system.";
        public static string PhoneRequired = "Phone number is required.";
        public static string OtpRequired = "Otp code is required.";
        public static string UserNotExist = "User does not exits in the system.";
        public static string UserNotAssociatedWithUserRole = "User does not associated with any user role.";
        public static string EmailRequired = "Email is required.";
        public static string RedirectUrlRequired = "Redirect Url is required.";   
        public static string UserNotFound = "User not found.";
        public static string PhoneNotVerified = "Phone is not verified.";
        public static string EmailNotBelongToUser = "Email does not belongs to this user.";
        public static string EmailVerificationNotSend = "Verification email not send. Please contact to the support team.";
        public static string EmailVerificationSendSuccess = "Email verification sent successfully. Please Check your inbox.";
        public static string EmailVerificationSuccess = "Email verified successfully.";
        public static string PhoneVerificationSuccess = "Phone number verified successfully.";
        public static string ChangePasswordFailed = "Login Failed: Incorrect phone or password!";
        public static string ChangePasswordSuccess = "Password updated successfully.";
        public static string EmailNotFound = "Email does not exist in the system";
        public static string ForgotPasswordFailed = "Something went wrong while sending password to your email. Please contact to the support team.";
        public static string ForgotPasswordSuccess = "Password has been sent to the given email address. Please check your inbox.";
        public static string OtpSendFailed = "Something went wrong while sending otp to your phone";
        public static string OtpSendSuccess = "6 digit code has been sent successfully.";
        public static string OtpVerifiedSuccess = "Phone verified successfully.";
        public static string OtpInvalid = "Verification code is incorrect.";
        public static string OtpNotFound = "Verification code is required.";
        public static string UserDelete = "User deleted successfully.";
        public static string UserUpdate = "User updated successfully.";
        public static string UserRetrived = "User retrived successfully.";
        public static string RoleNotFound = "Role not found.";
        public static string RoleDelete = "Role deleted successfully.";
        public static string RoleRetrived = "Roles retrived successfully.";
        public static string RoleInsert = "Role inserted successfully.";
        public static string RoleUpdate = "Role updated successfully.";
        public static string EmailPhoneRequired = "Email or phone is invalid";
        public static string PrivilegeNotFound = "Privileges not found.";
        public static string PrivilegeDelete = "Privileges deleted successfully.";
        public static string PrivilegeRetrived = "Privileges retrived successfully.";
        public static string PrivilegeInsert = "Privileges inserted successfully.";
        public static string PrivilegeUpdate = "Privileges updated successfully.";
        public static string PrivilegeExists = "Privileges already exists.";
        public static string ApplicationNotFound = "Applications not found.";
        public static string ApplicationDelete = "Applications deleted successfully.";
        public static string ApplicationRetrived = "Applications retrived successfully.";
        public static string ApplicationInsert = "Applications inserted successfully.";
        public static string ApplicationUpdate = "Applications updated successfully.";
        public static string ApplicationExists = " Applications already exists.";
        public static string PrivilegeAssociatedWithRole = "Privileges associated with role.";
        public static string PrivilegeAssociatedWithUserRole = "Privileges associated with user role.";
        public static string ApplicationAssociatedWithRole = "Application associated with role.";
        public static string ApplicationAssociatedWithUserRole = "Application associated with user role.";
        public static string UnknownApplication = "No Application is specified in request";
        public static string TokenDataNull = "Token data is null.";

    }
}       
