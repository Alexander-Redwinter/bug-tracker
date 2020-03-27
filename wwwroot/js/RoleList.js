﻿var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#DT_RolesLoad').DataTable({
        "ajax": {
            "url": "/Administration/GetRolesList",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "name", "width": "50%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                        <a href="/Administration/EditUsersInRole?id=${data}" class='btn btn-success text-white' style='cursor:pointer; width:auto;'>
                            Add or remove users
                        </a>
                        </div>`;
                }, "width": "40%"
            }
        ],
        "language": {
            "emptyTable": "no data found"
        },
        "width": "100%"
    });
}