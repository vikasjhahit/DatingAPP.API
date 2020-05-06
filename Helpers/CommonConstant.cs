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
        public static readonly string uploadPhotoSuccess = "Photo has been uploaded successfully.";
        public static readonly string uploadPhotoFail = "Could not add the photo.";
        public static readonly string uploadPhotoFormatError = "Could not add the photo. Please check the photo size.";
        public static readonly string setMainPhotoFail = "Could not set this photo as main photo.";
        public static readonly string setMainPhotoSuccess = "Success ! Photo set as main photo.";
        public static readonly string photoDeletedSuccess = "Photo has been deleted successfully.";
        public static readonly string photoDeletedFail = "Could not delete the photo.";
        public static readonly string mainPhotoDeleteMsg = "You cannot delete your main photo";
        public static readonly string unAuthorizedUser = "Unauthorized User.";
        public static readonly string AlreadyMainPhoto = "This is already the main photo.";
        public static readonly string alreadyLiked = "You already liked this user.";
        public static readonly string noMessage = "No Message";
        public static readonly string msgDeleted = "Message has been deleted successfully.";
        public static readonly string msgDeleteFail = "Error in deleting message.";

        public static readonly string registrationPath = "/api/users/getcountrylist";

    }
}
