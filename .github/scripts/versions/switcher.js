// Version switcher for the documentation sites published from this repository.
//
// Injected into every page of every built version by build_versioned_docs.py,
// which also hosts this file at <package root>/_versions/ next to
// <package root>/versions.json. Everything here is derived from that: the
// package root is the parent of this script's own URL, the current version is
// the first path segment under that root, and the page within the version is
// whatever follows — so switching keeps you on the same page when it exists in
// the other version, and falls back to that version's front page when it does
// not. Nothing is configured per package.
(function () {
  "use strict";

  var script = document.querySelector('script[src$="_versions/switcher.js"]');
  if (!script) return;
  var root = new URL("..", new URL(script.getAttribute("src"), location.href)).href;

  var here = location.href.split("#")[0].split("?")[0];
  if (here.indexOf(root) !== 0) return;
  var rel = here.slice(root.length);                 // e.g. "0.2/methods/x.html"
  var seg = rel.split("/")[0];

  fetch(root + "versions.json", { cache: "no-cache" })
    .then(function (r) { return r.ok ? r.json() : Promise.reject(r.status); })
    .then(function (versions) {
      var latest = versions.filter(function (v) { return v.latest; })[0];
      var current = versions.filter(function (v) { return v.path === seg + "/"; })[0];
      var atRoot = !current;                       // the root copy is the latest release
      if (atRoot) current = latest;
      var subpath = atRoot ? rel : rel.slice(seg.length + 1);

      // --- the selector, in furo's sidebar under the project title ---------
      var wrap = document.createElement("div");
      wrap.className = "version-switcher";
      var label = document.createElement("label");
      label.textContent = "Version";
      var select = document.createElement("select");
      select.setAttribute("aria-label", "Documentation version");
      versions.forEach(function (v) {
        var o = document.createElement("option");
        o.value = v.path;
        o.textContent = v.name;
        o.selected = v === current;
        select.appendChild(o);
      });
      select.addEventListener("change", function () {
        var target = root + select.value + subpath;
        fetch(target, { method: "HEAD" })
          .then(function (r) { location.href = r.ok ? target : root + select.value; })
          .catch(function () { location.href = root + select.value; });
      });
      label.appendChild(select);
      wrap.appendChild(label);

      var brand = document.querySelector(".sidebar-brand");
      if (brand && brand.parentNode) {
        brand.parentNode.insertBefore(wrap, brand.nextSibling);
      } else {
        wrap.className += " version-switcher--floating";
        document.body.appendChild(wrap);
      }

      // --- a notice on anything that is not the latest release -------------
      if (current && !current.latest) {
        var banner = document.createElement("div");
        banner.className = "version-banner";
        banner.setAttribute("role", "note");
        var text = document.createElement("span");
        text.innerHTML = current.tag
          ? "This page documents version <strong>" + current.version + "</strong>."
          : "This is the <strong>unreleased</strong> development version.";
        var link = document.createElement("a");
        link.href = root + subpath;
        link.textContent = "Go to the latest release (" + latest.version + ")";
        banner.appendChild(text);
        banner.appendChild(link);
        var article = document.querySelector("article[role=main]")
          || document.querySelector("article")
          || document.querySelector("main")
          || document.body;
        article.insertBefore(banner, article.firstChild);
      }
    })
    .catch(function () { /* no versions.json: nothing to switch between */ });
})();
