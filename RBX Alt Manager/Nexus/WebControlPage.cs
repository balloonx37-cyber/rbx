namespace RBX_Alt_Manager.Nexus
{
    // The full markup/script for the "/control" web page is kept as one inline string
    // (rather than a separate .html asset) so it always ships inside the built exe and
    // Update.zip with zero extra packaging steps - the same reasoning Nexus.lua's own
    // "serve the file that ships with this build" comment in AccountControl.OpenServer
    // gives for avoiding an extra loose file dependency.
    internal static class WebControlPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>Nexus Web Control</title>
<style>
  :root {
    color-scheme: dark;
  }
  * { box-sizing: border-box; }
  body {
    margin: 0;
    font-family: Segoe UI, Arial, sans-serif;
    background: #111318;
    color: #e6e6e6;
  }
  header {
    padding: 16px 20px;
    border-bottom: 1px solid #2a2d35;
  }
  header h1 {
    margin: 0;
    font-size: 18px;
    font-weight: 600;
  }
  #status {
    font-size: 12px;
    color: #8a8f98;
    margin-top: 4px;
  }
  main {
    padding: 16px 20px;
  }
  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 13px;
  }
  th {
    text-align: left;
    color: #8a8f98;
    font-weight: 500;
    padding: 8px 10px;
    border-bottom: 1px solid #2a2d35;
  }
  td {
    padding: 8px 10px;
    border-bottom: 1px solid #1c1e24;
    vertical-align: middle;
  }
  .dot {
    display: inline-block;
    width: 9px;
    height: 9px;
    border-radius: 50%;
    margin-right: 6px;
  }
  .online { background: #22c55e; }
  .offline { background: #6b7280; }
  .jobid {
    font-family: Consolas, monospace;
    font-size: 11px;
    color: #b8bcc4;
    display: inline-flex;
    align-items: center;
    gap: 6px;
  }
  .copy-btn {
    background: #2a2d35;
    color: #b8bcc4;
    border: none;
    border-radius: 3px;
    padding: 2px 6px;
    font-size: 11px;
    cursor: pointer;
  }
  .copy-btn:hover { background: #353944; }
  .autorejoin-cell {
    display: flex;
    align-items: center;
    gap: 6px;
  }
  .row-input {
    width: 100%;
    min-width: 160px;
    background: #1a1c22;
    border: 1px solid #2a2d35;
    color: #e6e6e6;
    border-radius: 4px;
    padding: 6px 8px;
    font-family: Consolas, monospace;
    font-size: 12px;
  }
  .row-controls {
    display: flex;
    gap: 6px;
  }
  button {
    background: #2563eb;
    color: white;
    border: none;
    border-radius: 4px;
    padding: 7px 14px;
    font-size: 12px;
    cursor: pointer;
    white-space: nowrap;
  }
  button:hover { background: #1d4ed8; }
  button:disabled { background: #374151; cursor: not-allowed; }
  .msg {
    font-size: 11px;
    margin-top: 4px;
    min-height: 14px;
  }
  .msg.ok { color: #4ade80; }
  .msg.err { color: #f87171; }
  #tokenGate {
    padding: 40px 20px;
    text-align: center;
  }
  #tokenGate input {
    background: #1a1c22;
    border: 1px solid #2a2d35;
    color: #e6e6e6;
    border-radius: 4px;
    padding: 8px 10px;
    width: 280px;
    font-family: Consolas, monospace;
  }
  .hidden { display: none; }
  .empty {
    color: #6b7280;
    padding: 20px;
    text-align: center;
  }
  .bulk-bar {
    display: flex;
    align-items: center;
    gap: 8px;
    background: #1a1c22;
    border: 1px solid #2a2d35;
    border-radius: 6px;
    padding: 10px 12px;
    margin-bottom: 12px;
  }
  .bulk-bar .bulk-count {
    font-size: 12px;
    color: #8a8f98;
    white-space: nowrap;
  }
  .bulk-bar input {
    flex: 1;
    min-width: 160px;
    background: #0f1115;
    border: 1px solid #2a2d35;
    color: #e6e6e6;
    border-radius: 4px;
    padding: 7px 8px;
    font-family: Consolas, monospace;
    font-size: 12px;
  }
  .bulk-bar .msg {
    margin-top: 0;
  }
  .machine-summary {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-bottom: 12px;
  }
  .machine-card {
    background: #1a1c22;
    border: 1px solid #2a2d35;
    border-radius: 6px;
    padding: 10px 14px;
    min-width: 160px;
    cursor: pointer;
    user-select: none;
    transition: border-color 0.15s, background 0.15s;
  }
  .machine-card:hover { border-color: #3b82f6; }
  .machine-card.active { border-color: #2563eb; background: #1e2a3d; }
  .machine-card .machine-name {
    font-size: 13px;
    font-weight: 600;
    margin-bottom: 4px;
  }
  .machine-card .machine-stats {
    font-size: 12px;
    color: #8a8f98;
  }
  .machine-card .machine-stats .online-count {
    color: #4ade80;
  }
  .machine-card .machine-stats .offline-count {
    color: #6b7280;
  }
</style>
</head>
<body>
<header>
  <h1>Nexus Web Control</h1>
  <div id=""status"">Connecting...</div>
</header>
<main>
  <div id=""tokenGate"" class=""hidden"">
    <p>Enter the control token (found in Account Control &rarr; Settings):</p>
    <input id=""tokenInput"" type=""text"" placeholder=""token"" />
    <br /><br />
    <button id=""tokenSubmit"">Continue</button>
  </div>
  <div id=""bulkBar"" class=""bulk-bar hidden"">
    <span class=""bulk-count"" id=""bulkCount"">0 selected</span>
    <input id=""bulkJobId"" type=""text"" placeholder=""job id for selected accounts"" />
    <button id=""bulkTeleport"">Teleport Selected</button>
    <button id=""bulkAutoRejoin"">Enable Auto Re-join</button>
    <button id=""bulkAutoRejoinOff"">Disable Auto Re-join</button>
  </div>
  <div class=""msg"" id=""bulkMsg""></div>
  <div id=""machineSummary"" class=""machine-summary hidden""></div>
  <table id=""accountsTable"" class=""hidden"">
    <thead>
      <tr>
        <th><input type=""checkbox"" id=""selectAll"" /></th>
        <th>Machine</th>
        <th>Account</th>
        <th>Game</th>
        <th>Job ID</th>
        <th>Players</th>
        <th>Alive</th>
        <th>Money</th>
        <th>Bank</th>
        <th>Teleport to Job ID</th>
        <th>Auto Re-join</th>
      </tr>
    </thead>
    <tbody id=""accountsBody""></tbody>
  </table>
  <div id=""emptyMsg"" class=""empty hidden"">No accounts connected yet.</div>
</main>
<script>
(function () {
  var STORAGE_KEY = 'nexusControlToken';
  var params = new URLSearchParams(window.location.search);
  var token = params.get('token') || localStorage.getItem(STORAGE_KEY) || '';

  var statusEl = document.getElementById('status');
  var gateEl = document.getElementById('tokenGate');
  var tableEl = document.getElementById('accountsTable');
  var bodyEl = document.getElementById('accountsBody');
  var emptyEl = document.getElementById('emptyMsg');

  function saveToken(t) {
    token = t;
    localStorage.setItem(STORAGE_KEY, t);
  }

  document.getElementById('tokenSubmit').addEventListener('click', function () {
    var v = document.getElementById('tokenInput').value.trim();
    if (!v) return;
    saveToken(v);
    gateEl.classList.add('hidden');
    poll();
  });

  if (token) saveToken(token);

  function authHeaders() {
    return { 'X-Control-Token': token };
  }

  function escapeHtml(s) {
    return String(s == null ? '' : s).replace(/[&<>""']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '""': '&quot;', ""'"": '&#39;' }[c];
    });
  }

  // Keyed by username - remembers whatever the user currently has typed in each row's
  // ""Teleport to Job ID"" input across polls. Without this, the 5s poll's re-render kept
  // overwriting the input back to the server's stored autoRejoinJobId, so clearing or
  // editing the field appeared to silently ""bounce back"" a moment later.
  var jobInputDrafts = {};

  // Keyed by ""machine|username"" (not just username - relay mode can show the same
  // username connected from two different machines) - remembers which rows are checked
  // across the 5s poll re-render, the same reason jobInputDrafts exists above. A plain
  // Set survives render() being called again since it's never cleared there, only when
  // a row's own checkbox is unticked or a bulk action finishes.
  var selectedKeys = {};

  // The accounts array from the most recent render() - selectAll and the bulk action
  // buttons live outside #accountsBody (see their wiring further down) so they need their
  // own reference to the current rows rather than reading them back out of the DOM.
  var lastAccounts = [];
  // The filtered subset actually rendered in the table (same as lastAccounts when no
  // machine filter is active) - bulk actions and selectAll must only touch visible rows.
  var lastVisible = [];

  // The filtered subset actually shown in the table (may equal lastAccounts when no
  // machine filter is active) - selectAll must only toggle these visible rows, otherwise
  // ticking ""select all"" while a machine filter is on would silently check hidden rows
  // from other machines too.
  // When set (via clicking a machine card), render() only shows that machine's rows and
  // the card gets a highlighted border. Clicking the same card again clears the filter.
  var machineFilter = null;

  function rowKey(a) {
    return (a.machine || '') + '|' + a.username;
  }

  function visibleAccounts() {
    return machineFilter
      ? lastAccounts.filter(function (a) { return (a.machine || '(this machine)') === machineFilter; })
      : lastAccounts;
  }

  function updateBulkBar() {
    var keys = Object.keys(selectedKeys).filter(function (k) { return selectedKeys[k]; });
    var bar = document.getElementById('bulkBar');
    var selectAll = document.getElementById('selectAll');
    var visible = visibleAccounts();

    selectAll.checked = visible.length > 0 && keys.length === visible.length;
    selectAll.indeterminate = keys.length > 0 && keys.length < visible.length;

    if (keys.length === 0) {
      bar.classList.add('hidden');
      return;
    }

    bar.classList.remove('hidden');
    document.getElementById('bulkCount').textContent = keys.length + ' selected';
  }

  function renderMachineSummary(accounts) {
    var el = document.getElementById('machineSummary');

    // Group by machine name (relay mode) - in non-relay mode every row has machine=null
    // so they all collapse into one ""(this machine)"" card.
    var byMachine = {};
    var order = [];

    accounts.forEach(function (a) {
      var m = a.machine || '(this machine)';

      if (!byMachine[m]) {
        byMachine[m] = { total: 0, online: 0 };
        order.push(m);
      }

      byMachine[m].total++;

      if (a.status === 'Online') byMachine[m].online++;
    });

    var anyOnline = order.some(function (m) { return byMachine[m].online > 0; });

    el.innerHTML =
      '<div class=""machine-summary-title"">' +
      order.length + ' machine' + (order.length === 1 ? '' : 's') + ' – ' +
      (anyOnline ? '<span class=""online-count"">' +
        order.filter(function (m) { return byMachine[m].online > 0; }).length +
        ' online</span>' : '<span class=""offline-count"">all offline</span>') +
      '</div>' +
      order.map(function (m) {
        var s = byMachine[m];
        var active = machineFilter === m ? ' active' : '';

        return '<div class=""machine-card' + active + '"" data-machine=""' + escapeHtml(m) + '"">' +
          '<div class=""machine-name""><span class=""dot ' + (s.online > 0 ? 'online' : 'offline') + '""></span>' + escapeHtml(m) + '</div>' +
          '<div class=""machine-stats"">' +
          '<span class=""online-count"">' + s.online + ' online</span>' +
          ' / ' + s.total + ' account' + (s.total === 1 ? '' : 's') +
          '</div>' +
          '</div>';
      }).join('');

    el.classList.remove('hidden');

    el.querySelectorAll('.machine-card').forEach(function (card) {
      card.addEventListener('click', function () {
        var m = this.getAttribute('data-machine');
        machineFilter = (machineFilter === m) ? null : m;
        render(lastAccounts);
      });
    });
  }

  function render(accounts) {
    if (!accounts.length) {
      tableEl.classList.add('hidden');
      emptyEl.classList.remove('hidden');
      document.getElementById('machineSummary').classList.add('hidden');
      return;
    }

    emptyEl.classList.add('hidden');
    tableEl.classList.remove('hidden');

    lastAccounts = accounts;
    renderMachineSummary(accounts);

    // When a machine card is selected, only show that machine's rows in the table -
    // the summary cards always show every machine so the user can switch or clear the filter.
    var visible = machineFilter
      ? accounts.filter(function (a) { return (a.machine || '(this machine)') === machineFilter; })
      : accounts;

    // Capture the live DOM values (not just jobInputDrafts) right before they get destroyed
    // by the innerHTML replacement below, so an in-progress edit from the instant before this
    // poll landed isn't lost either.
    visible.forEach(function (a, i) {
      var existing = document.getElementById('jobinput-' + i);
      if (existing) jobInputDrafts[a.username] = existing.value;
    });

    // Drop selection entries for accounts that are no longer in this poll's list (e.g. they
    // disconnected), so the bulk count/bar doesn't keep counting a row that's gone.
    var currentKeys = {};
    accounts.forEach(function (a) { currentKeys[rowKey(a)] = true; });
    Object.keys(selectedKeys).forEach(function (k) {
      if (!currentKeys[k]) delete selectedKeys[k];
    });

    bodyEl.innerHTML = visible.map(function (a, i) {
      var dotClass = a.status === 'Online' ? 'online' : 'offline';
      var players = (a.players >= 0 && a.maxPlayers >= 0) ? (a.players + '/' + a.maxPlayers) : '';
      var game = a.placeName || a.placeId || '';
      var draftJobId = jobInputDrafts.hasOwnProperty(a.username) ? jobInputDrafts[a.username] : (a.autoRejoinJobId || '');
      var checked = selectedKeys[rowKey(a)] ? ' checked' : '';

      return '' +
        '<tr>' +
        '<td><input type=""checkbox"" class=""row-select"" id=""select-' + i + '""' + checked + ' /></td>' +
        '<td>' + escapeHtml(a.machine || '') + '</td>' +
        '<td><span class=""dot ' + dotClass + '""></span>' + escapeHtml(a.username) + '</td>' +
        '<td>' + escapeHtml(game) + '</td>' +
        '<td class=""jobid"" title=""' + escapeHtml(a.jobId) + '"">' +
        '<span>' + escapeHtml((a.jobId || '').slice(0, 8)) + (a.jobId ? '&hellip;' : '') + '</span>' +
        (a.jobId ? '<button class=""copy-btn"" id=""copy-' + i + '"" data-jobid=""' + escapeHtml(a.jobId) + '"">Copy</button>' : '') +
        '</td>' +
        '<td>' + escapeHtml(players) + '</td>' +
        '<td>' + (a.health >= 0 ? (a.isAlive ? '✅ ' + a.health : '💀') : '') + '</td>' +
        '<td>' + (a.money >= 0 ? '$' + a.money.toLocaleString() : '') + '</td>' +
        '<td>' + (a.bankMoney >= 0 ? '$' + a.bankMoney.toLocaleString() : '') + '</td>' +
        '<td>' +
        '<div class=""row-controls"">' +
        '<input class=""row-input"" id=""jobinput-' + i + '"" placeholder=""target job id"" value=""' + escapeHtml(draftJobId) + '"" />' +
        '<button id=""go-' + i + '"" data-username=""' + escapeHtml(a.username) + '"">Go</button>' +
        '</div>' +
        '<div class=""msg"" id=""msg-' + i + '""></div>' +
        '</td>' +
        '<td>' +
        '<div class=""autorejoin-cell"">' +
        '<input type=""checkbox"" id=""autorejoin-' + i + '""' + (a.autoRejoin ? ' checked' : '') + ' />' +
        '</div>' +
        '<div class=""msg"" id=""rejoinmsg-' + i + '""></div>' +
        '</td>' +
        '</tr>';
    }).join('');

    visible.forEach(function (a, i) {
      document.getElementById('jobinput-' + i).addEventListener('input', function () {
        jobInputDrafts[a.username] = this.value;
      });
    });

    visible.forEach(function (a, i) {
      document.getElementById('go-' + i).addEventListener('click', function () {
        var btn = this;
        var input = document.getElementById('jobinput-' + i);
        var msg = document.getElementById('msg-' + i);
        var jobId = input.value.trim();

        if (!jobId) {
          msg.textContent = 'Enter a Job ID first';
          msg.className = 'msg err';
          return;
        }

        btn.disabled = true;
        msg.textContent = 'Sending...';
        msg.className = 'msg';

        fetch('/control/api/teleport', {
          method: 'POST',
          headers: Object.assign({ 'Content-Type': 'application/json' }, authHeaders()),
          body: JSON.stringify({ username: a.username, jobId: jobId, machine: a.machine })
        }).then(function (r) { return r.json(); }).then(function (result) {
          msg.textContent = result.message || (result.success ? 'Sent' : 'Failed');
          msg.className = 'msg ' + (result.success ? 'ok' : 'err');
        }).catch(function (e) {
          msg.textContent = 'Request failed: ' + e.message;
          msg.className = 'msg err';
        }).finally(function () {
          btn.disabled = false;
        });
      });

      var copyBtn = document.getElementById('copy-' + i);

      if (copyBtn) {
        copyBtn.addEventListener('click', function () {
          var jobId = this.getAttribute('data-jobid');

          (navigator.clipboard ? navigator.clipboard.writeText(jobId) : Promise.reject()).then(function () {
            copyBtn.textContent = 'Copied';
            setTimeout(function () { copyBtn.textContent = 'Copy'; }, 1200);
          }).catch(function () {
            // Clipboard API unavailable (e.g. non-HTTPS context) - fall back to a hidden
            // textarea + execCommand, the standard workaround for older/restricted browsers.
            var ta = document.createElement('textarea');
            ta.value = jobId;
            ta.style.position = 'fixed';
            ta.style.opacity = '0';
            document.body.appendChild(ta);
            ta.select();
            try { document.execCommand('copy'); } catch (e) { }
            document.body.removeChild(ta);
            copyBtn.textContent = 'Copied';
            setTimeout(function () { copyBtn.textContent = 'Copy'; }, 1200);
          });
        });
      }

      document.getElementById('autorejoin-' + i).addEventListener('change', function () {
        var checkbox = this;
        var input = document.getElementById('jobinput-' + i);
        var msg = document.getElementById('rejoinmsg-' + i);
        var jobId = input.value.trim();

        if (checkbox.checked && !jobId) {
          msg.textContent = 'Enter a target Job ID first';
          msg.className = 'msg err';
          checkbox.checked = false;
          return;
        }

        checkbox.disabled = true;
        msg.textContent = 'Saving...';
        msg.className = 'msg';

        fetch('/control/api/autorejoin', {
          method: 'POST',
          headers: Object.assign({ 'Content-Type': 'application/json' }, authHeaders()),
          body: JSON.stringify({ username: a.username, machine: a.machine, enabled: checkbox.checked, jobId: jobId })
        }).then(function (r) { return r.json(); }).then(function (result) {
          msg.textContent = result.message || (result.success ? 'Saved' : 'Failed');
          msg.className = 'msg ' + (result.success ? 'ok' : 'err');
          if (!result.success) checkbox.checked = !checkbox.checked;
        }).catch(function (e) {
          msg.textContent = 'Request failed: ' + e.message;
          msg.className = 'msg err';
          checkbox.checked = !checkbox.checked;
        }).finally(function () {
          checkbox.disabled = false;
        });
      });

      document.getElementById('select-' + i).addEventListener('change', function () {
        selectedKeys[rowKey(a)] = this.checked;
        updateBulkBar();
      });
    });

    updateBulkBar();
  }

  // selectAll/bulkJobId/bulkTeleport/bulkAutoRejoin live outside #accountsBody, so unlike the
  // per-row listeners above (which get rewired on every render() since innerHTML destroys and
  // recreates those nodes) these only need to be wired up once.
  document.getElementById('selectAll').addEventListener('change', function () {
    var checked = this.checked;

    visibleAccounts().forEach(function (a) { selectedKeys[rowKey(a)] = checked; });

    document.querySelectorAll('.row-select').forEach(function (cb) { cb.checked = checked; });

    updateBulkBar();
  });

  function selectedAccounts() {
    return lastAccounts.filter(function (a) { return selectedKeys[rowKey(a)]; });
  }

  var bulkButtonIds = ['bulkTeleport', 'bulkAutoRejoin', 'bulkAutoRejoinOff'];

  // Shared by all three bulk buttons - fires the same per-account request the single-row
  // controls already use (via the existing /control/api/teleport and /control/api/autorejoin
  // endpoints - there's no separate bulk endpoint on the server), just fanned out over every
  // selected row with Promise.all so they run concurrently instead of waiting on each other,
  // then reports how many succeeded. requireJobId is false for ""Disable Auto Re-join"" - the
  // server only requires a jobId when actually enabling it (see HandleWebControlAutoRejoin's
  // `if (Enabled && string.IsNullOrEmpty(JobId))`), turning it off doesn't need one.
  function runBulk(path, buildBody, requireJobId) {
    var jobId = document.getElementById('bulkJobId').value.trim();
    var msg = document.getElementById('bulkMsg');
    var targets = selectedAccounts();

    if (requireJobId && !jobId) {
      msg.textContent = 'Enter a Job ID first';
      msg.className = 'msg err';
      return;
    }

    if (targets.length === 0) {
      msg.textContent = 'No accounts selected';
      msg.className = 'msg err';
      return;
    }

    bulkButtonIds.forEach(function (id) { document.getElementById(id).disabled = true; });
    msg.textContent = 'Sending to ' + targets.length + ' account(s)...';
    msg.className = 'msg';

    Promise.all(targets.map(function (a) {
      return fetch(path, {
        method: 'POST',
        headers: Object.assign({ 'Content-Type': 'application/json' }, authHeaders()),
        body: JSON.stringify(buildBody(a, jobId))
      }).then(function (r) { return r.json(); }).then(function (result) {
        return !!result.success;
      }).catch(function () {
        return false;
      });
    })).then(function (results) {
      var okCount = results.filter(Boolean).length;

      msg.textContent = okCount + '/' + targets.length + ' succeeded';
      msg.className = 'msg ' + (okCount === targets.length ? 'ok' : 'err');
    }).finally(function () {
      bulkButtonIds.forEach(function (id) { document.getElementById(id).disabled = false; });
    });
  }

  document.getElementById('bulkTeleport').addEventListener('click', function () {
    runBulk('/control/api/teleport', function (a, jobId) {
      return { username: a.username, jobId: jobId, machine: a.machine };
    }, true);
  });

  document.getElementById('bulkAutoRejoin').addEventListener('click', function () {
    runBulk('/control/api/autorejoin', function (a, jobId) {
      return { username: a.username, machine: a.machine, enabled: true, jobId: jobId };
    }, true);
  });

  document.getElementById('bulkAutoRejoinOff').addEventListener('click', function () {
    runBulk('/control/api/autorejoin', function (a) {
      return { username: a.username, machine: a.machine, enabled: false, jobId: '' };
    }, false);
  });

  function poll() {
    fetch('/control/api/accounts', { headers: authHeaders() }).then(function (r) {
      if (r.status === 401) {
        statusEl.textContent = 'Invalid token';
        gateEl.classList.remove('hidden');
        tableEl.classList.add('hidden');
        emptyEl.classList.add('hidden');
        throw new Error('unauthorized');
      }
      return r.json();
    }).then(function (accounts) {
      statusEl.textContent = 'Connected – updated ' + new Date().toLocaleTimeString();
      render(accounts);
    }).catch(function () {
      /* keep last render on transient errors, status already set for 401 case above */
    });
  }

  if (!token) {
    statusEl.textContent = 'Token required';
    gateEl.classList.remove('hidden');
  } else {
    poll();
  }

  setInterval(function () {
    if (token) poll();
  }, 3000);
})();
</script>
</body>
</html>";
    }
}
