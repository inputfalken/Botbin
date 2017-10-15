using System.ComponentModel.DataAnnotations;

namespace Botbin.UserTracking.UserEvent.Enums {
    public enum UserAction {
        [Display(Name = "Start Game")] StartGame = 0,
        [Display(Name = "Quit Game")] QuitGame = 1,
        [Display(Name = "Logged Off")] LogOff = 2,
        [Display(Name = "Logged In")] LogIn = 3,
        [Display(Name = "Sent Message")] SentMessage = 4
    }
}