// Home OS - Shell client behavior.

// Command palette (Ctrl+K / Cmd+K): jump to any enabled module or run a
// search. Module targets come from the registry-generated data island, so a
// new module appears here automatically (Docs/00_Specifikacija_Izvor.md,
// "automatski vidljiva u komandnoj paleti").
(function () {
    var dataEl = document.getElementById('commandPaletteData');
    var config = window.homeOsCommandPalette;
    if (!dataEl || !config || typeof bootstrap === 'undefined') {
        return; // Not authenticated / palette not present on this page.
    }

    var targets = [];
    try {
        targets = JSON.parse(dataEl.textContent) || [];
    } catch (e) {
        targets = [];
    }

    var modalEl = document.getElementById('commandPalette');
    var inputEl = document.getElementById('commandPaletteInput');
    var resultsEl = document.getElementById('commandPaletteResults');
    var modal = new bootstrap.Modal(modalEl);
    var activeIndex = 0;
    var rendered = [];

    function render(query) {
        var q = (query || '').trim().toLowerCase();
        var items = targets.filter(function (t) {
            return !q || t.name.toLowerCase().indexOf(q) !== -1;
        }).map(function (t) {
            return { label: t.icon + ' ' + t.name, url: t.url };
        });

        // Always offer a full-text search for whatever was typed.
        if (q) {
            items.push({
                label: '🔎 ' + config.searchPrefix + ' "' + query.trim() + '"',
                url: config.searchUrl + '?q=' + encodeURIComponent(query.trim())
            });
        }

        rendered = items;
        activeIndex = 0;
        resultsEl.innerHTML = '';
        items.forEach(function (item, i) {
            var li = document.createElement('li');
            li.className = 'list-group-item list-group-item-action' + (i === activeIndex ? ' active' : '');
            li.textContent = item.label;
            li.style.cursor = 'pointer';
            li.addEventListener('click', function () { go(i); });
            resultsEl.appendChild(li);
        });
    }

    function go(i) {
        if (rendered[i]) {
            window.location.href = rendered[i].url;
        }
    }

    function highlight() {
        Array.prototype.forEach.call(resultsEl.children, function (li, i) {
            li.classList.toggle('active', i === activeIndex);
        });
    }

    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'K')) {
            e.preventDefault();
            render('');
            modal.show();
        }
    });

    modalEl.addEventListener('shown.bs.modal', function () {
        inputEl.value = '';
        render('');
        inputEl.focus();
    });

    inputEl.addEventListener('input', function () {
        render(inputEl.value);
    });

    inputEl.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            activeIndex = Math.min(activeIndex + 1, rendered.length - 1);
            highlight();
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            activeIndex = Math.max(activeIndex - 1, 0);
            highlight();
        } else if (e.key === 'Enter') {
            e.preventDefault();
            go(activeIndex);
        }
    });
})();
