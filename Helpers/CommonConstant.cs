using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public static class CommonConstant
    {
        public static readonly string userDetailUpdateSuccess = "Profile updated successfully.";
        public static readonly string userDetailUpdateFail = "Error occured while updating profile.";
        public static readonly string Success= "Success";
        public static readonly string Failure = "Failure";
        public static readonly string userAlreadyExist = "User already exist. please login to continue.";
        public static readonly string userRegistrationFail = "Error occured in registration process.";
        public static readonly string userRegistrationSuccess = "Registration successfull. Please login to continue.";
        public static readonly string invalidUserNamePassword = "User Name or password is incorrect. Please enter correct username and password.";
        public static readonly string loginFailed = "Error occured while login. Please try again.";

        public static readonly string registrationPath = "/api/users/getcountrylist";

    }
}
