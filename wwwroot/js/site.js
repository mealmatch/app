// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const apiPostCall = async (url, data) => {
    const options = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    }

    const resp = await fetch(url, options)
    return await resp.json()
}

const create = async (name, shuffle, voteAll, numVotesToWin, numWinsToEnd, options) => {
    return await apiPostCall('/api/create', {
        name: name,
        shuffle: shuffle,
        numVotesToWin: voteAll ? 999999 : numVotesToWin,
        numWinsToEnd: voteAll ? 999999 : numWinsToEnd,
        options: options
    })
}

const yelp = async (location, search, page) => {
    const body = { location: location, search: search, page: page }
    return await apiPostCall('/api/yelp', body)
}
