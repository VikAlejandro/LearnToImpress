using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using System;
using Firebase.Extensions;

public class FirebaseController : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject singupPanel;
    public GameObject profilePanel;
    public GameObject forgetPasswordPanel;
    public GameObject notificationPanel;

    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;

    public TMP_InputField signupEmail;
    public TMP_InputField signupPassword;
    public TMP_InputField signupCPassword;
    public TMP_InputField signupUserName;

    public TMP_InputField forgetPassEmail;

    public TMP_Text notif_Tittle_Text;
    public TMP_Text notif_Message_Text;
    public TMP_Text profileUserName_Text;
    public TMP_Text profileUserEmail_Text;

    public Toggle rememberMe;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    //bool isSignIn = false;


    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
          var dependencyStatus = task.Result;
          if (dependencyStatus == Firebase.DependencyStatus.Available) {
            // Create and hold a reference to your FirebaseApp,
            // where app is a Firebase.FirebaseApp property of your application class.
               InitializeFirebase();

            // Set a flag here to indicate whether Firebase is ready to use by your app.
          } else {
            UnityEngine.Debug.LogError(System.String.Format(
              "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
          }
        });
    }


    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        singupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }


    public void OpenSingUpPanel()
    {
        loginPanel.SetActive(false);
        singupPanel.SetActive(true);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }


    public void OpenProfilePanel()
    {
        loginPanel.SetActive(false);
        singupPanel.SetActive(false);
        profilePanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }


    public void OpenForgetPassPanel()
    {
        loginPanel.SetActive(false);
        singupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }


    public void LoginUser()
    {
        string email = loginEmail.text.Trim();
        string password = loginPassword.text;

        if (string.IsNullOrEmpty(email))
        {
            showNotificationMessage("Error", "Escribe tu correo electrónico");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            showNotificationMessage("Error", "Escribe tu contraseña");
            return;
        }

        SignInUser(email, password);
    }


    public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) ||
            string.IsNullOrEmpty(signupPassword.text) ||
            string.IsNullOrEmpty(signupUserName.text))
        {
            showNotificationMessage("Error", "Campos vacíos");
            return;
        }

        if (signupPassword.text != signupCPassword.text)
        {
            showNotificationMessage("Error", "Las contraseñas no coinciden");
            return;
        }

        createUser(
            signupEmail.text,
            signupPassword.text,
            signupUserName.text
        );
    }


    public void forgetPass()
    {
        if (string.IsNullOrEmpty(forgetPassEmail.text))
        {
            showNotificationMessage("Error", "Campo vacío");
            return;
        }

        forgetPasswordSubmit(forgetPassEmail.text);

        // Recuperación de contraseña
    }


    private void showNotificationMessage(string title, string message)
    {
        notif_Tittle_Text.text = title;
        notif_Message_Text.text = message;

        notificationPanel.SetActive(true);
    }


    public void CloseNotif_Panel()
    {
        notif_Tittle_Text.text = "";
        notif_Message_Text.text = "";

        notificationPanel.SetActive(false);
    }


    public void LogOut()
    {
        if (auth != null)
        {
            auth.SignOut();
        }

        profilePanel.SetActive(false);

        profileUserName_Text.text = "";
        profileUserEmail_Text.text = "";

        OpenLoginPanel();
    }
    

    void createUser(string email, string password, string UserName)
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError(
                    "CreateUserWithEmailAndPasswordAsync was canceled."
                );

                showNotificationMessage(
                    "Error",
                    "Registro cancelado"
                );

                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError(
                    "CreateUserWithEmailAndPasswordAsync encountered an error: "
                    + task.Exception
                );

                foreach (Exception exception in
                         task.Exception.Flatten().InnerExceptions)
                {
                    FirebaseException firebaseEx =
                        exception as FirebaseException;

                    if (firebaseEx != null)
                    {
                        var errorCode =
                            (AuthError)firebaseEx.ErrorCode;

                        Debug.LogError(
                            "Firebase Error Code: " +
                            firebaseEx.ErrorCode
                        );

                        Debug.LogError(
                            "Auth Error: " +
                            errorCode
                        );

                        Debug.LogError(
                            "Firebase Message: " +
                            firebaseEx.Message
                        );

                        if (firebaseEx.Message.Contains(
                                "password is invalid"))
                        {
                            showNotificationMessage(
                                "Error",
                                "La contraseña no cumple los requisitos"
                            );
                        }
                        else
                        {
                            showNotificationMessage(
                                "Error",
                                GetErrorMessage(errorCode)
                            );
                        }
                    }
                }

                return;
            }

            AuthResult result = task.Result;

            Debug.LogFormat(
                "Firebase user created successfully: {0} ({1})",
                result.User.DisplayName,
                result.User.UserId
            );

            UpdateUserProfile(UserName);
        });
    }


    public void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                showNotificationMessage(
                    "Error",
                    "Inicio de sesión cancelado"
                );
                return;
            }

            if (task.IsFaulted)
            {
                foreach (Exception exception in
                         task.Exception.Flatten().InnerExceptions)
                {
                    FirebaseException firebaseEx =
                        exception as FirebaseException;

                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;

                        Debug.LogError("Firebase Error Code: " + firebaseEx.ErrorCode);
                        Debug.LogError("Auth Error: " + errorCode);
                        Debug.LogError("Firebase Message: " + firebaseEx.Message);

                        if (errorCode == AuthError.Failure &&
                            firebaseEx.Message.Contains("internal error"))
                        {
                            showNotificationMessage(
                                "Error",
                                "Correo o contraseña incorrectos"
                            );
                        }
                        else
                        {
                            showNotificationMessage(
                                "Error",
                                GetErrorMessage(errorCode)
                            );
                        }
                    }
                }

                return;
            }

            AuthResult result = task.Result;

            Debug.Log(
                "Usuario inició sesión: " +
                result.User.Email
            );

            profileUserName_Text.text =
                result.User.DisplayName;

            profileUserEmail_Text.text =
                result.User.Email;

            OpenProfilePanel();
        });
    }


    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChanged;

        AuthStateChanged(this, null);
    }


    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn =
                user != auth.CurrentUser &&
                auth.CurrentUser != null &&
                auth.CurrentUser.IsValid();

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                //isSignIn = true;
            }
        }
    }


    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
            auth = null;
        }
    }


    void UpdateUserProfile(string UserName)
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            Firebase.Auth.UserProfile profile =
                new Firebase.Auth.UserProfile
                {
                    DisplayName = UserName,
                    PhotoUrl = new System.Uri(
                        "https://placehold.co/600x400"
                    )
                };

            user.UpdateUserProfileAsync(profile)
                .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError(
                        "UpdateUserProfileAsync was canceled."
                    );
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "UpdateUserProfileAsync encountered an error: "
                        + task.Exception
                    );
                    return;
                }

                Debug.Log("User profile updated successfully.");

                showNotificationMessage(
                    "Alert",
                    "Account Successfully Created"
                );
            });
        }
    }

    //bool isSigned = false;


    /*
    void Update()
    {
        if(isSignIn)
        {
            if(!isSigned)
            {
                isSigned = true;
                profileUserName_Text.text = user.DisplayName;
                profileUserEmail_Text.text = user.Email;
                OpenProfilePanel();
            }
        }
    }
     */

    private static string GetErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                return "Ya existe la cuenta con credenciales diferentes";

            case AuthError.MissingPassword:
                return "Hace falta el password";

            case AuthError.WeakPassword:
                return "El password es débil";

            case AuthError.WrongPassword:
                return "El password es incorrecto";

            case AuthError.InvalidCredential:
                return "El password es incorrecto";

            case AuthError.EmailAlreadyInUse:
                return "Ya existe una cuenta con ese correo electrónico";

            case AuthError.InvalidEmail:
                return "Correo electrónico inválido";

            case AuthError.MissingEmail:
                return "Hace falta el correo electrónico";

            case AuthError.UserNotFound:
                return "No existe una cuenta con ese correo electrónico";

            default:
                return "Ocurrió un error";
        }
    }


    void forgetPasswordSubmit( string forgetPasswordEmail)
    {
        auth.SendPasswordResetEmailAsync(forgetPasswordEmail).ContinueWithOnMainThread(task => {
            
            if(task.IsCanceled){
                Debug.LogError("SendPasswordResetEmailAsync was canceled");

            }

            if(task.IsFaulted){
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    FirebaseException firebaseEx =
                        exception as FirebaseException;

                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;

                        Debug.LogError("Firebase Error Code: " + firebaseEx.ErrorCode);
                        Debug.LogError("Auth Error: " + errorCode);
                        Debug.LogError("Firebase Message: " + firebaseEx.Message);

                        if (errorCode == AuthError.Failure &&
                            firebaseEx.Message.Contains("internal error"))
                        {
                            showNotificationMessage(
                                "Error",
                                "Correo incorrecto o inexistente"
                            );
                        }
                        else
                        {
                            showNotificationMessage(
                                "Error",
                                GetErrorMessage(errorCode)
                            );
                        }
                    }
                }
            }

            showNotificationMessage("Alert","Correo enviado!");

        }
        );
    }
}