<template>
  <div id="app">
    <template v-if="!loggedIn">
      <div>
        用户名：
        <input type="text" v-model="username" />
      </div>
      <button @click="login()">Login</button>
    </template>
    <template v-else>
      loggedin!
      <button @click="signout()">Signout</button>
    </template>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:8060/loginhub")
  .build();

export default Vue.extend({
  name: "App",
  data() {
    return {
      username: "",
      loginId: "",
      loggedIn: false
    };
  },

  async mounted() {
    const loginId = localStorage.getItem("loginId");
    const username = localStorage.getItem("username");
    if (loginId && username) {
      this.loggedIn = true;
      this.username = username;
      this.loginId = loginId;
      const response = await fetch(
        `http://localhost:8060/api/Account/CheckLogin?username=${username}`
      );
      if (response.ok) {
        if (loginId !== (await response.text())) {
          this.signout();
        }
      }
    }
  },

  methods: {
    async login() {
      const response = await fetch(
        `http://localhost:8060/api/Account/Login?username=${this.username}`
      );
      if (response.ok) {
        this.loginId = await response.text();
        localStorage.setItem("username", this.username);
        localStorage.setItem("loginId", this.loginId);

        this.loggedIn = true;

        const handler = (username: string, loginId: string) => {
          if (username === this.username && loginId !== this.loginId) {
            this.signout();
          }
        };
        connection.on("login", handler.bind(this));

        connection.start().catch(err => console.log(err));
      }
    },
    async signout() {
      await fetch(
        `http://localhost:8060/api/Account/Signout?username=${this.username}&loginId=${this.loginId}`
      );
      localStorage.removeItem("loginId");
      localStorage.removeItem("username");
      this.loginId = "";
      this.username = "";
      connection.stop();
      this.loggedIn = false;
    }
  }
});
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
